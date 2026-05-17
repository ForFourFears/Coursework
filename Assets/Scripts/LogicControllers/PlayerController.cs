using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Coursework.AnimationControllers;
using Coursework.LogicControllers.ActionStateMachines;
using Coursework.LogicControllers.ActionTriggerHubs;
using Scripts;

namespace Coursework.LogicControllers
{
    public class PlayerController : MonoBehaviour, IActionSystemProvider<PlayerActionState, PlayerActionTrigger>
    {
        #region Public part
        public IActionStateMachine<PlayerActionState> StateMachine => stateMachine;
        public IActionTriggerHub<PlayerActionTrigger> TriggerHub => triggerHub;
        public ObservableSMBsHub AnimationEventHub { get; private set; }
        public bool IsGrounded => Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer) != null;
        
        #endregion

        #region Serialize part
        [Header("Movement")]
        [field: SerializeField] public Rigidbody2D Rigidbody {  get; private set; }
        [SerializeField] private float _runSpeedModifier = 1.4f;
        [SerializeField] private float _crouchSpeedModifier = 1f;
        [SerializeField] private float _jumpForceModifier = 5f;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask _groundLayer;
        #endregion

        #region Private part
        private InputSystemActions inputActions;

        private readonly ActionStateMachine<PlayerActionState> stateMachine = new();
        private readonly ActionTriggerHub<PlayerActionTrigger> triggerHub = new();

        private float currentSpeedModifier;
        private Vector2 moveInput;

        
        #endregion
        
        private void Awake()
        {
            inputActions = new InputSystemActions();
            Rigidbody = Rigidbody != null ? Rigidbody : GetComponent<Rigidbody2D>();
            AnimationEventHub = AnimationEventHub != null ? AnimationEventHub : GetComponent<ObservableSMBsHub>();

            currentSpeedModifier = _runSpeedModifier;
        }

        private void OnEnable()
        {
            inputActions.Enable();

            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;

            inputActions.Player.Jump.performed += OnJumpPerformed;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;

            inputActions.Player.Jump.performed -= OnJumpPerformed;

            inputActions.Disable();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            UpdateState();
        }

        #region On input events
        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (IsGrounded)
            {
                Rigidbody.AddForce(new Vector2(0, _jumpForceModifier), ForceMode2D.Impulse);
            }
        }
        #endregion

        private void UpdateState()
        {

            PlayerActionState targetState;

            if (IsGrounded)
            {
                if (moveInput.y < 0)
                {
                    targetState = PlayerActionState.Crouch;
                }
                else
                {
                    targetState = PlayerActionState.Locomotion;
                }
            }
            else
            {
                targetState = PlayerActionState.Air;
            }
            stateMachine.ChangeState(targetState);
        }

        private void ApplyMovement()
        {
            Rigidbody.linearVelocity = new Vector2(moveInput.x * currentSpeedModifier, Rigidbody.linearVelocity.y);
            if(moveInput.x != 0)
            {
                float direction = Mathf.Sign(moveInput.x);
                Vector3 scale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Abs(scale.x) * direction, scale.y, scale.z);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}