using Coursework.AnimationControllers.Core;
using Coursework.AnimationControllers.Implementations;
using Coursework.EnumsCreatures.Skeleton;
using Coursework.LogicControllers.ActionExecutionSystems.Core;
using Coursework.LogicControllers.ActionExecutionSystems.Implementations;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.ActionStateMachines.Implementations;
using Coursework.LogicControllers.AttackSystems;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.LogicControllers.MovementSystems;
using Coursework.ScriptableObjects;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Coursework.LogicControllers
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class SkeletonController : MonoBehaviour, IBaseController<SkeletonActions>, IMovementContext, IBaseEntityContext, ITransformComponent, IAttacker, IDamageable
    {
        #region Public part
        public bool IsGrounded { get; private set; }
        public float FacingSign { get; private set; }

        public Vector2 MoveInput { get; set; }
        public Vector2 SlopeDirection { get; private set; }
        public float SlopeAngle { get; private set; }

        public Transform Transform => transform;

        public IActionStateMachine<SkeletonStates, SkeletonActions> ActionStateMachine => actionStateMachine;
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

        [Header("Animator Controller")]
        [SerializeField] private Animator _animator;

        [Header("Configs")]
        [SerializeField] private SkeletonConfig _skeletonConfig;

        [Header("Debug")]
        [SerializeField] private Transform infoPosition;
        [SerializeField] private Transform SlopeDirectionPosition;
        #endregion

        #region Private part
        private ObservableSMBsHandler observableSMBsHandler;
        private SkeletonActionStateMachine actionStateMachine;
        private SkeletonActionExecutionSystem actionExecutionSystem;
        private MovementSystem movementSystem;
        private ModifierSystem modifierSystem;
        private SkeletonAnimatorController animatorController;
        private HealthSystem healthSystem;
        #endregion

        private void Awake()
        {
            Rigidbody = Rigidbody != null ? Rigidbody : GetComponent<Rigidbody2D>();

            modifierSystem = new();
            movementSystem = new(this, this, modifierSystem);

            _animator = _animator != null ? _animator : GetComponent<Animator>();

            observableSMBsHandler = new(_animator);
            actionStateMachine = new(this, this, modifierSystem, _skeletonConfig, observableSMBsHandler);
            actionExecutionSystem = new(this, this, ActionStateMachine, _skeletonConfig);

            animatorController = new(Rigidbody, _animator, ActionStateMachine, observableSMBsHandler);

            healthSystem = new(_skeletonConfig.Health, _skeletonConfig.Health);

        }

        private void OnEnable()
        {


            actionStateMachine.Subscribe();
            actionExecutionSystem.Subscribe();
            animatorController.Subscribe();
        }

        private void OnDisable()
        {
            actionStateMachine.Unsubscribe();
            actionExecutionSystem.Unsubscribe();
            animatorController.Unsubscribe();



        }

        private void Update()
        {
            UpdateFacingDirection();
            animatorController.Update();
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();
            UpdateSlopeDirection();


            actionStateMachine.Update(Time.fixedDeltaTime);

            movementSystem.FixedUpdate();
            actionExecutionSystem.Update();
        }

        private void UpdateFacingDirection()
        {
            if (MoveInput.x != 0)
            {
                float moveDirection = Mathf.Sign(MoveInput.x);
                if (moveDirection != FacingSign) actionStateMachine.TryExecuteAction(SkeletonActions.TurnAround);
            }
            if (transform.localScale.x != 0) FacingSign = Mathf.Sign(transform.localScale.x);
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
                SlopeAngle = 0;
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

        public bool TryExecuteAction(SkeletonActions action)
        {
            return actionStateMachine.TryExecuteAction(action);
        }

        public void OnHit(Collider2D target, HitInfo hitInfo)
        {
            actionExecutionSystem.OnHit(target, hitInfo);
        }

        public void TakeDamage(float damage)
        {
            healthSystem.Health -= damage;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (actionStateMachine != null && modifierSystem != null && infoPosition != null)
            {
                GUIStyle labelStyle = new()
                {
                    fontSize = 32
                };
                labelStyle.normal.textColor = Color.white;
                labelStyle.alignment = TextAnchor.MiddleCenter;


                Handles.Label(infoPosition.position,
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
        }
#endif
    }
}
