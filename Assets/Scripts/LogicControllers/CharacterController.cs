using Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using Coursework.LogicControllers.ActionBuffers;

namespace Coursework.LogicControllers
{
	interface IEntityContext
	{
        public bool IsGrounded { get; }
        public bool IsCrouchIntentHeld { get; }
		public bool IsCeilingAbove { get; }
    }

    interface IMovementContext
    {
        public Vector2 MoveInput { get; }

        public Rigidbody2D Rigidbody { get; }
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController : MonoBehaviour, IEntityContext, IMovementContext
    {
        #region Public part
        public bool IsGrounded { get; private set; }
        public bool IsCrouchIntentHeld { get; private set; }
        public bool IsCeilingAbove { get; private set; }
        #endregion

        #region Serialize part
        [Header("Movement")]
        [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }
        [SerializeField] private float _runSpeedModifier = 1.4f;
        [SerializeField] private float _crouchSpeedModifier = 1f;
        [SerializeField] private float _jumpForceModifier = 5f;

        [Header("Grounded Check")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Ceiling Check")]
        [SerializeField] private Transform _ceilingCheck;
        [SerializeField] private float _ceilingCheckRadius = 0.1f;
        [SerializeField] private LayerMask _ceilingLayer;
        #endregion

        #region Private part
        private InputSystemActions inputSystemActions;
        private ActionBuffer actionBuffer;
        private ActionStateMachine actionStateMachine;
        private ActionExecutionSystem actionExecutionSystem;
        private MovementSystem movementSystem;
        private ModifierSystem modifierSystem;

        public Vector2 MoveInput { get; private set; }
        #endregion

        private void Awake()
        {
            Rigidbody = Rigidbody != null ? Rigidbody : GetComponent<Rigidbody2D>();

            inputSystemActions = new();
            actionBuffer = new();
            modifierSystem = new();
            movementSystem = new(this, modifierSystem);
            actionStateMachine = new(modifierSystem);
            actionExecutionSystem = new(actionStateMachine);
        }

        private void OnEnable()
        {
            inputSystemActions.Enable();

            inputSystemActions.Player.Move.performed += OnMove;
            inputSystemActions.Player.Move.canceled += OnMove;
            inputSystemActions.Player.Jump.performed += OnJump;
        }

        private void OnDisable()
        {
            inputSystemActions.Player.Move.performed -= OnMove;
            inputSystemActions.Player.Move.canceled -= OnMove;
            inputSystemActions.Player.Jump.performed -= OnJump;

            inputSystemActions.Disable();
        }

        private void Update()
        {
            IsCrouchIntentHeld = CheckCrouchIntent();
            actionBuffer.Update(Time.deltaTime);
            actionStateMachine.Update();
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();
            IsCeilingAbove = CheckCeiling();

            ActionRequest action = actionBuffer.GetNewestActionRequest();
            if(action.Action != PlayerActions.None && actionStateMachine.TryExecuteAction(action))
            {
                actionBuffer.RemoveAction(action);
            }

            movementSystem.FixedUpdate();
            actionExecutionSystem.FixedUpdate();
        }

        #region On Event Callbacks
        private void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            PlayerActions action = PlayerActions.Jump;
            actionBuffer.AddAction(action);
        }
        #endregion

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
    }
}