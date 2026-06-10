using Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using Coursework.ScriptableObjects;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionBuffers;
using Coursework.LogicControllers.ActionExecutionSystems;
using Coursework.LogicControllers.ActionStateMachines;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.AnimationControllers;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Coursework.LogicControllers
{
	public interface IEntityContext
	{
        public bool IsGrounded { get; }
        public bool IsCrouched { get; }
		public bool IsCeilingAbove { get; }
        public bool IsAttacking { get; }
        public bool IsRolling { get; }
        public bool IsDashing { get; }
    }

    public interface IMovementContext
    {
        public Vector2 MoveInput { get; }

        public Rigidbody2D Rigidbody { get; }
    }
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IEntityContext, IMovementContext/*, IActionStateMachineProvider<KnightActionStates,  KnightActions>*/
    {
        #region Public part
        public bool IsGrounded { get; private set; }
        public bool IsCrouched { get; private set; }
        public bool IsCeilingAbove { get; private set; }
        public bool IsAttacking { get; private set; }
        public bool IsRolling { get; private set; }
        public bool IsDashing { get; private set; }

        public IActionStateMachine<KnightActionStates, KnightActions> ActionStateMachine { get => actionStateMachine; }
        #endregion

        #region Serialize part
        [Header("Movement")]
        [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }

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

        public Vector2 MoveInput { get; private set; }
        #endregion

        private void Awake()
        {
            Rigidbody = Rigidbody != null ? Rigidbody : GetComponent<Rigidbody2D>();

            inputSystemActions = new();
            actionBuffer = new();
            modifierSystem = new();
            movementSystem = new(this, modifierSystem);

            _animator = _animator != null ? _animator : GetComponent<Animator>();

            observableSMBsHandler = new(_animator);
            actionStateMachine = new(this, this, modifierSystem, _knightConfig, observableSMBsHandler);
            actionExecutionSystem = new(this, ActionStateMachine, _knightConfig);

            animatorController = new (this, Rigidbody, _animator, ActionStateMachine, observableSMBsHandler);

        }

        private void OnEnable()
        {
            inputSystemActions.Enable();

            inputSystemActions.Player.Move.performed += OnMove;
            inputSystemActions.Player.Move.canceled += OnMove;
            inputSystemActions.Player.Crouch.performed += OnCrouch;
            inputSystemActions.Player.Crouch.canceled += OnCrouch;
            inputSystemActions.Player.Jump.performed += OnJump;
            inputSystemActions.Player.Attack.performed += OnAttack;
            inputSystemActions.Player.Attack.canceled += OnAttack;

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
            inputSystemActions.Player.Attack.performed -= OnAttack;
            inputSystemActions.Player.Attack.canceled -= OnAttack;

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

            actionStateMachine.Update();

            if (IsAttacking)
            {
                KnightActions actionAttack = KnightActions.Attack;
                actionBuffer.AddAction(actionAttack, 0.2f);
            }

            ActionRequest action = actionBuffer.GetOldestActionRequest();
            
            if(action.Action != KnightActions.None && actionStateMachine.TryExecuteAction(action.Action))
            {
                actionBuffer.RemoveAction(action);
            }

            movementSystem.FixedUpdate();
            actionExecutionSystem.Update();
        }

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

        private void OnAttack(InputAction.CallbackContext context)
        {
            //KnightActions action = KnightActions.Attack;
            //actionBuffer.AddAction(action, 0.2f);
            IsAttacking = context.ReadValueAsButton();
        }

        private void OnRoll(InputAction.CallbackContext context)
        {
            IsRolling = context.ReadValueAsButton();
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            IsDashing = context.ReadValueAsButton();
        }
        #endregion

        private void UpdateFacingDirection()
        {
            if (MoveInput.x != 0)
            {
                float direction = Mathf.Sign(MoveInput.x);
                Vector3 scale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(scale.x) * direction, scale.y, scale.z);
            }
        }
        private bool CheckGrounded()
        {
            return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer) != null;
        }

        private bool CheckCrouchIntent()
        {
            return MoveInput.y < -0.1;
        }

        private bool CheckCeiling()
        {
            return Physics2D.OverlapCircle(_ceilingCheck.position, _ceilingCheckRadius, _ceilingLayer) != null;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (actionStateMachine != null && modifierSystem != null)
            {
                Vector3 textPosition = transform.position + Vector3.up * 0.5f;

                Handles.Label(textPosition, $"{actionStateMachine.CurrentState}, {modifierSystem.StateModifier}, IsAttacking: {IsAttacking}");
            }

            if (_groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
            }
            if (_ceilingCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_ceilingCheck.position, _ceilingCheckRadius);
            }
        }
        #endif

    }
}