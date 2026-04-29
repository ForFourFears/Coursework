using Coursework.Scripts.Animation;
using Coursework.Scripts.LogicController.ActionStateMachine;
using Coursework.Scripts.LogicController.ActionTriggerHub;
using Scripts;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Coursework.Scripts.LogicController
{
    public class PlayerController : MonoBehaviour, IActionSystemProvider<PlayerActionState, PlayerActionTrigger>
    {
        #region Public part
        public IActionStateMachine<PlayerActionState> StateMachine => stateMachine;
        public IActionTriggerHub<PlayerActionTrigger> TriggerHub => triggerHub;
        #endregion

        #region Serialize part
        [Header("Movement")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private float _moveSpeed = 1.4f;
        [SerializeField] private float _speedInTurn = 0f;
        [SerializeField] private float _jumpForce;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask _groundLayer;
        #endregion

        #region Private part
        private InputSystemActions inputActions;

        private readonly ActionStateMachine<PlayerActionState> stateMachine = new();
        private readonly ActionTriggerHub<PlayerActionTrigger> triggerHub = new();
        private ObservableSMBsHub observableSMBsHub;

        private float currentSpeed;
        private Vector2 moveInput;
        private Vector2 lastMoveInput = new(1, 0);

        private bool IsGrounded => Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer) != null;
        private bool IsCheckUpdateDirection => moveInput.x != 0 && Math.Sign(moveInput.x) != Math.Sign(lastMoveInput.x);
        private bool isBlockedUpdateStateMachine;
        private float intendedScaleX;
        #endregion

        private void Awake()
        {
            inputActions = new InputSystemActions();
            _rb = _rb != null ? _rb : GetComponent<Rigidbody2D>();
            observableSMBsHub = observableSMBsHub != null ? observableSMBsHub : GetComponent<ObservableSMBsHub>();

            currentSpeed = _moveSpeed;
        }

        private void OnEnable()
        {
            inputActions.Enable();

            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;

            inputActions.Player.Jump.performed += OnJumpPerformed;

            stateMachine[PlayerActionState.TurnAround].Entered += OnEnterTurn;
            observableSMBsHub["TurnAround"].ExitState += OnExitTurn;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;

            inputActions.Player.Jump.performed -= OnJumpPerformed;

            stateMachine[PlayerActionState.TurnAround].Entered -= OnEnterTurn;
            observableSMBsHub["TurnAround"].ExitState -= OnExitTurn;

            inputActions.Disable();
        }

        private void FixedUpdate()
        {
            Move();
            UpdateState();
        }

        #region On events
        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (IsGrounded)
            {
                _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
            }
        }

        private void OnEnterTurn()
        {
            currentSpeed = _speedInTurn;
            isBlockedUpdateStateMachine = true;
        }

        private void OnExitTurn()
        {
            UpdateDirection();
            currentSpeed = _moveSpeed;
            isBlockedUpdateStateMachine = false;
        }
        #endregion

        private void UpdateState()
        {
            if (isBlockedUpdateStateMachine)
            {
                if (moveInput.x != 0)
                {
                    intendedScaleX = Mathf.Sign(moveInput.x);
                    return;
                }
            }

            PlayerActionState targetState;
            if (IsGrounded)
            {
                if (IsCheckUpdateDirection)
                {
                    intendedScaleX = Mathf.Sign(moveInput.x);
                    lastMoveInput = new Vector2(intendedScaleX, 0);
                    stateMachine.ChangeState(PlayerActionState.TurnAround);
                    return;
                }
                else
                {
                    targetState = (_rb.linearVelocityX != 0) ? PlayerActionState.Run : PlayerActionState.Idle;
                }
            }
            else
            {
                if (IsCheckUpdateDirection)
                {
                    intendedScaleX = Mathf.Sign(moveInput.x);
                    UpdateDirection();
                }
                targetState = (_rb.linearVelocityY > 0) ? PlayerActionState.Jump : PlayerActionState.Fall;
            }
            stateMachine.ChangeState(targetState);
        }

        private void Move()
        {
            _rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, _rb.linearVelocity.y);
        }

        private void UpdateDirection()
        {
            if (moveInput.x != 0)
            {
                intendedScaleX = Mathf.Sign(moveInput.x);
            }
            float localScaleX = MathF.Abs(transform.localScale.x);
            gameObject.transform.localScale = new Vector3(localScaleX * Mathf.Sign(intendedScaleX), gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            lastMoveInput = new Vector2(intendedScaleX, 0);
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}