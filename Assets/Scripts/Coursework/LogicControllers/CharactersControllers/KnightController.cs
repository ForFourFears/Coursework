using UnityEngine;
using Coursework.ScriptableObjects;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionExecutionSystems.Core;
using Coursework.LogicControllers.ActionExecutionSystems.Implementations;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.ActionStateMachines.Implementations;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.LogicControllers.MovementSystems;
using Coursework.AnimationControllers.Core;
using Coursework.AnimationControllers.Implementations;
using Coursework.LogicControllers.AttackSystems;
using System;
using Coursework.Managers;
using System.Collections;



#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Coursework.LogicControllers.CharactersControllers
{
    #region Interfaces
    public interface IEntityContext
    {
        public bool IsAlive { get; }
        public bool IsGrounded { get; }
        public float FacingSign { get; }
    }

	public interface ICrouchInfo : IEntityContext
    {
        public bool IsCrouched { get; }
		public bool IsCeilingAbove { get; }
    }

    public interface IMovementContext
    {
        public Vector2 MoveInput { get; }
        public float FacingSign { get; }

        public Vector2 SlopeDirection { get; }
        public float SlopeAngle { get; }
        public float MaxSlopeAngle { get; }

        public Rigidbody2D Rigidbody { get; }

        public float MaxFallSpeed { get; }
    }

    public interface ITransformComponent
    {
        public Transform Transform { get; }
    }

    public interface IController<in TAction> : IEntityContext
        where TAction : Enum
    {
        public Vector2 MoveInput { get; set; }
        public bool TryExecuteAction(TAction action);
    }

    public interface IKnightController : IController<KnightActions>
    {
        public bool IsCrouched { get; set; }
    }

    public interface IDamageable
    {
        public void TakeDamage(float damage);
    }
    #endregion

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class KnightController : MonoBehaviour, IKnightController, ICrouchInfo, IMovementContext, ITransformComponent, IAttacker, IDamageable/*, IActionStateMachineProvider<KnightStates,  KnightActions>*/, ISceneInitializable
    {
        #region Public part
        public bool IsAlive => actionStateMachine.CurrentState != KnightStates.Death;
        public bool IsGrounded { get; private set; }
        public float FacingSign { get; private set; }
        public bool IsCrouched { get; set; }
        public bool IsCeilingAbove { get; private set; }

        public Vector2 MoveInput { get; set; }
        public Vector2 SlopeDirection { get; private set; }
        public float SlopeAngle { get; private set; }

        public Transform Transform => transform;

        public IHealth Health => healthSystem;
        public IActionStateMachine<KnightStates, KnightActions> ActionStateMachine => actionStateMachine;
        #endregion

        #region Serialize part
        [Header("Movement")]
        [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }

        [field: Min(5)]
        [field: SerializeField] public float MaxFallSpeed { get; private set; } = 5;

        [Header("Slope Detection")]
        [field: SerializeField] public float MaxSlopeAngle { get; private set; }
        [SerializeField] private Transform _normalOrigin;
        [SerializeField] private float _normalVectorLength = 0.3f;
        [SerializeField] private float _vectorDistortion;
        [SerializeField] private float _snapDistance = 0.2f;

        [Header("Grounded Check")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Ceiling Check")]
        [SerializeField] private Transform _ceilingCheck;
        [SerializeField] private float _ceilingCheckRadius = 0.1f;
        [SerializeField] private LayerMask _ceilingLayer;

        [Header("Animator Controller")]
        [SerializeField] private Animator _animator;

        [Header("Debug")]
        [SerializeField] private Transform infoPosition;
        [SerializeField] private Transform SlopeDirectionPosition;
        #endregion

        #region Private part
        private ObservableSMBsHandler observableSMBsHandler;
        private KnightActionStateMachine actionStateMachine;
        private KnightActionExecutionSystem actionExecutionSystem;
        private MovementSystem movementSystem;
        private ModifierSystem modifierSystem;
        private KnightAnimatorController animatorController;
        private HealthSystem healthSystem;

        private IEntityDataHandler<KnightStates, KnightActions> knightConfig;

        private readonly WaitForSeconds deathDelay = new(1.5f);

        private bool isInitialized;
        private bool isSubscribed;
        #endregion

        public void Initialize()
        {
            G.Player = gameObject;

            if (GameSessionManager.Instance.RespawnPosition.HasValue)
            {
                transform.position = GameSessionManager.Instance.RespawnPosition.Value;
            }

            knightConfig = GameSessionManager.Instance.KnightData;

            healthSystem = new(knightConfig.Health, knightConfig.Health);

            Rigidbody = Rigidbody != null ? Rigidbody : GetComponent<Rigidbody2D>();
            
            modifierSystem = new();
            movementSystem = new(this, this, modifierSystem);

            _animator = _animator != null ? _animator : GetComponent<Animator>();

            observableSMBsHandler = new(_animator);
            actionStateMachine = new(this, this, modifierSystem, healthSystem, observableSMBsHandler, knightConfig);
            actionExecutionSystem = new(this, this, this, ActionStateMachine, knightConfig);

            animatorController = new (Rigidbody, _animator, ActionStateMachine, observableSMBsHandler);

            isInitialized = true;
            OnEnable();
        }

        private void OnEnable()
        {
            if (!isInitialized || isSubscribed) return;
            
            actionStateMachine.Subscribe();
            actionExecutionSystem.Subscribe();
            animatorController.Subscribe();

            healthSystem.HealthChanged += OnDeath;

            isSubscribed = true;
        }

        private void OnDisable()
        {
            if (!isInitialized || !isSubscribed) return;

            healthSystem.HealthChanged -= OnDeath;

            actionStateMachine.Unsubscribe();
            actionExecutionSystem.Unsubscribe();
            animatorController.Unsubscribe();

            isSubscribed = false;
        }

        private void OnDestroy()
        {
            if (G.Player == gameObject)
            {
                G.Player = null;
            }
        }

        private void Update()
        {
            if (!isInitialized) return;

            UpdateFacingDirection();
            animatorController.Update();
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            IsGrounded = CheckGrounded();
            IsCeilingAbove = CheckCeiling();
            UpdateSlopeDirection();

            actionStateMachine.Update(Time.fixedDeltaTime);

            movementSystem.FixedUpdate();
        }

        public bool TryExecuteAction(KnightActions action)
        {
            return actionStateMachine.TryExecuteAction(action);
        }

        private void UpdateFacingDirection()
        {
            if (MoveInput.x != 0)
            {
                float moveDirection = Mathf.Sign(MoveInput.x);
                if (!Mathf.Approximately(FacingSign, moveDirection)) actionStateMachine.TryExecuteAction(KnightActions.TurnAround);
            }
            if (transform.localScale.x != 0) FacingSign = Mathf.Sign(transform.localScale.x);
        }

        public void OnHit(Collider2D target, HitInfo hitInfo)
        {
            actionExecutionSystem.OnHit(target, hitInfo);
        }

        public void TakeDamage(float damage)
        {
            actionStateMachine.TakeDamage(damage);
        }

        private bool CheckGrounded()
        {
            bool isGround = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer) != null;

            return isGround;
        }

        private void UpdateSlopeDirection()
        {
            Vector2 def = Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(_normalOrigin.position, Vector2.down, _normalVectorLength + _snapDistance, _groundLayer);

            if (hit.normal == Vector2.zero)
            {
                SlopeDirection = def;
                SlopeAngle = 90;
                return;
            }

            SlopeAngle = Vector2.Angle(Vector2.up, hit.normal);

            if (SlopeAngle <= MaxSlopeAngle)
            {
                if (SlopeAngle > 10f)
                {
                    Vector2 pureSlopeDir = new(hit.normal.y, -hit.normal.x);

                    SlopeDirection = new Vector2(
                        pureSlopeDir.x,
                        pureSlopeDir.y - _vectorDistortion * FacingSign
                    ).normalized;

                    return;
                }
                
            }

            SlopeDirection = def;
        }

        private bool CheckCeiling()
        {
            return Physics2D.OverlapCircle(_ceilingCheck.position, _ceilingCheckRadius, _ceilingLayer) != null;
        }

        private void OnDeath(float health, float maxHealth, float delta)
        {
            if (health <= 0) StartCoroutine(Death());
        }

        private IEnumerator Death()
        {
            yield return deathDelay;
            SceneLoader.Instance.ReloadCurrentScene();
        }
        
        #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (actionStateMachine != null && modifierSystem != null && healthSystem != null && infoPosition != null)
            {
                GUIStyle labelStyle = new()
                {
                    fontSize = 32
                };
                labelStyle.normal.textColor = Color.white; 
                labelStyle.alignment = TextAnchor.MiddleCenter; 


                Handles.Label(infoPosition.position, 
                    $"Health: {healthSystem.Health}\\{healthSystem.MaxHealth}\n" +
                    $"{actionStateMachine.CurrentState}, {modifierSystem.StateModifier},\n" +
                    $"SlopeDirection: {SlopeDirection}\n" +
                    $"SlopeAngle: {Vector2.Angle(Vector2.right, SlopeDirection)}\n" +
                    $"IsGrounded: {IsGrounded}", labelStyle);
            }

            if (SlopeDirection != Vector2.zero && SlopeDirectionPosition != null)
            {
                Vector3 startPoint = SlopeDirectionPosition.position;
                Vector3 endPoint = startPoint + (Vector3)SlopeDirection * FacingSign;
                Handles.color = Color.green;
                Handles.DrawLine(startPoint, endPoint, 4f);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(endPoint, 0.1f);
            }

            if (_groundCheck != null)
            {
                if (IsGrounded) Gizmos.color = Color.green;
                else Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
            }

            if (_normalOrigin != null)
            {
                Vector3 startPoint = _normalOrigin.position;
                Vector3 endPoint = startPoint + Vector3.down * (_normalVectorLength + _snapDistance);
                Handles.color = Color.green;
                Handles.DrawLine(startPoint, endPoint, 6f);
            }

            if (_ceilingCheck != null)
            {
                if (IsCeilingAbove) Gizmos.color = Color.green;
                else Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_ceilingCheck.position, _ceilingCheckRadius);
            }
        }
        #endif
    }
}