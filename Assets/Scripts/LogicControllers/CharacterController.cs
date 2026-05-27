using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionBuffers;
using Coursework.LogicControllers.ActionExecutionSystems;
using Coursework.LogicControllers.ActionStateMachines;
using Coursework.LogicControllers.ModifierSystems;
using Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Coursework.LogicControllers
{
	public interface IEntityContext
	{
        public bool IsGrounded { get; }
        public bool IsCrouchIntentHeld { get; }
		public bool IsCeilingAbove { get; }
    }

    public interface IMovementContext
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
        private KnightActionStateMachine actionStateMachine;
        private KnightActionExecutionSystem actionExecutionSystem;
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
            actionStateMachine = new(this, this, modifierSystem);
            actionExecutionSystem = new(this, actionStateMachine);
        }

        private void OnEnable()
        {
            inputSystemActions.Enable();

            inputSystemActions.Player.Move.performed += OnMove;
            inputSystemActions.Player.Move.canceled += OnMove;
            inputSystemActions.Player.Jump.performed += OnJump;

            actionExecutionSystem.Subscribe();

        }

        private void OnDisable()
        {
            actionExecutionSystem.Unsubscribe();

            inputSystemActions.Player.Move.performed -= OnMove;
            inputSystemActions.Player.Move.canceled -= OnMove;
            inputSystemActions.Player.Jump.performed -= OnJump;

            inputSystemActions.Disable();
        }

        private void Update()
        {
            actionBuffer.Update(Time.deltaTime);
            UpdateFacingDirection();
        }

        private void FixedUpdate()
        {
            IsCrouchIntentHeld = CheckCrouchIntent();
            IsGrounded = CheckGrounded();
            IsCeilingAbove = CheckCeiling();

            actionStateMachine.Update();

            ActionRequest action = actionBuffer.GetNewestActionRequest();
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
            actionBuffer.AddAction(action);
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

        private void OnDrawGizmos()
        {
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
    }
}