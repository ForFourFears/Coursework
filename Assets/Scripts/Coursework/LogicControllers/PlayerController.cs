using Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using Coursework.ScriptableObjects;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionBuffers;
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


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Coursework.LogicControllers
{
    public interface IBaseEntityContext
    {
        public bool IsGrounded { get; }

        public float FacingSign { get; }
    }

	public interface IEntityContext : IBaseEntityContext
    {
        public bool IsCrouched { get; }
		public bool IsCeilingAbove { get; }
    }

    public interface IMovementContext
    {
        public Vector2 MoveInput { get; }
        public Vector2 SlopeDirection { get; }
        public float SlopeAngle { get; }
        public float MaxSlopeAngle { get; }
        public Rigidbody2D Rigidbody { get; }

        public float MaxFallSpeed { get; }
    }

    public interface IBaseController<TAction>
        where TAction : Enum
    {
        public Vector2 MoveInput { get; set; }

        public bool TryExecuteAction(TAction action);

        public float MaxSlopeAngle { get; }
    }

    public interface IDamageable
    {
        public void TakeDamage(float damage);
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IEntityContext, IMovementContext, IAttacker, IDamageable/*, IActionStateMachineProvider<KnightStates,  KnightActions>*/
    {
        #region Public part
        public bool IsGrounded { get; private set; }
        public bool IsCrouched { get; set; }
        public bool IsCeilingAbove { get; private set; }
        public float FacingSign { get; private set; }

        public Vector2 MoveInput { get; set; }
        public Vector2 SlopeDirection { get; private set; }
        public float SlopeAngle { get; private set; }

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

        [Header("Configs")]
        [SerializeField] private KnightConfig _knightConfig;

        [Header("Debug")]
        [SerializeField] private Transform infoPosition;
        [SerializeField] private Transform SlopeDirectionPosition;
        #endregion

        #region Private part
        private InputSystemActions inputSystemActions;
        private ActionBuffer actionBuffer;
        private ObservableSMBsHandler observableSMBsHandler;
        private KnightActionStateMachine actionStateMachine;
        private KnightActionExecutionSystem actionExecutionSystem;
        private MovementSystem movementSystem;
        private ModifierSystem modifierSystem;
        private KnightAnimatorController animatorController;
        private HealthSystem healthSystem;


        #endregion

        private void Awake()
        {
            Rigidbody = Rigidbody != null ? Rigidbody : GetComponent<Rigidbody2D>();

            inputSystemActions = new();
            actionBuffer = new();
            modifierSystem = new();
            movementSystem = new(this, this, modifierSystem);

            _animator = _animator != null ? _animator : GetComponent<Animator>();

            observableSMBsHandler = new(_animator);
            actionStateMachine = new(this, this, modifierSystem, _knightConfig, observableSMBsHandler);
            actionExecutionSystem = new(this, this, ActionStateMachine, _knightConfig);

            animatorController = new (Rigidbody, _animator, ActionStateMachine, observableSMBsHandler);

            healthSystem = new(_knightConfig.Health, _knightConfig.Health);

        }

        private void OnEnable()
        {
            inputSystemActions.Enable();

            inputSystemActions.Player.Move.performed += OnMove;
            inputSystemActions.Player.Move.canceled += OnMove;

            inputSystemActions.Player.Crouch.performed += OnCrouch;
            inputSystemActions.Player.Crouch.canceled += OnCrouch;

            inputSystemActions.Player.Jump.performed += OnJump;

            //inputSystemActions.Player.Attack.performed += OnAttack;

            //inputSystemActions.Player.Dash.performed += OnDash;

            //inputSystemActions.Player.Roll.performed += OnRoll;

            actionStateMachine.Subscribe();
            actionExecutionSystem.Subscribe();
            animatorController.Subscribe();

        }

        private void OnDisable()
        {
            actionStateMachine.Unsubscribe();
            actionExecutionSystem.Unsubscribe();
            animatorController.Unsubscribe();

            inputSystemActions.Player.Move.performed -= OnMove;
            inputSystemActions.Player.Move.canceled -= OnMove;

            inputSystemActions.Player.Crouch.performed -= OnCrouch;
            inputSystemActions.Player.Crouch.canceled -= OnCrouch;

            inputSystemActions.Player.Jump.performed -= OnJump;

            //inputSystemActions.Player.Attack.performed -= OnAttack;

            //inputSystemActions.Player.Dash.performed -= OnDash;

            //inputSystemActions.Player.Roll.performed -= OnRoll;


            inputSystemActions.Disable();
        }

        private void Update()
        {
            actionBuffer.Update(Time.deltaTime);
            UpdateFacingDirection();
            animatorController.Update();
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();
            IsCeilingAbove = CheckCeiling();
            FacingSign = Mathf.Sign(transform.localScale.x);
            UpdateSlopeDirection();

            actionStateMachine.Update(Time.fixedDeltaTime);

            KnightActions actionType = KnightActions.None;
            if (inputSystemActions.Player.Roll.IsPressed()) actionType = KnightActions.Roll;
            else if (inputSystemActions.Player.Attack.IsPressed()) actionType = KnightActions.Attack;
            else if (inputSystemActions.Player.Dash.IsPressed()) actionType = KnightActions.Dash;
 
            actionBuffer.AddAction(actionType, 0.2f);

            ActionRequest action = actionBuffer.GetOldestActionRequest();
            if(action.Action != KnightActions.None && actionStateMachine.TryExecuteAction(action.Action))
            {
                actionBuffer.RemoveAction(action);
            }

            movementSystem.FixedUpdate();
            actionExecutionSystem.Update();
        }

        //Этот регион должен быть в управлении игрока, а не контроллера
        #region On Event Callbacks
        private void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            KnightActions action = KnightActions.Jump;
            actionBuffer.AddAction(action, 0.2f);
        }

        private void OnCrouch(InputAction.CallbackContext context)
        {
            IsCrouched = context.ReadValueAsButton();
        }

        //private void OnAttack(InputAction.CallbackContext context)
        //{
        //    KnightActions actionAttack = KnightActions.Attack;
        //    actionBuffer.AddAction(actionAttack, 0.2f);
        //}

        //private void OnDash(InputAction.CallbackContext context)
        //{
        //    KnightActions actionAttack = KnightActions.Dash;
        //    actionBuffer.AddAction(actionAttack, 0.2f);
        //}

        //private void OnRoll(InputAction.CallbackContext context)
        //{
        //    KnightActions actionAttack = KnightActions.Roll;
        //    actionBuffer.AddAction(actionAttack, 0.2f);
        //}
        #endregion

        private void UpdateFacingDirection()
        {
            if (MoveInput.x != 0)
            {
                float moveDirection = Mathf.Sign(MoveInput.x);
                Vector3 facingDirection = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(facingDirection.x) * moveDirection, facingDirection.y, facingDirection.z);
            }
        }

        public void OnHit(Collider2D target, HitInfo hitInfo)
        {
            actionExecutionSystem.OnHit(target, hitInfo);
        }

        public void TakeDamage(float damage)
        {
            healthSystem.Health -= damage;
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

        private bool CheckCeiling()
        {
            return Physics2D.OverlapCircle(_ceilingCheck.position, _ceilingCheckRadius, _ceilingLayer) != null;
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

            if (SlopeDirection != Vector2.zero)
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