using Coursework.Controller;
using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Scripts
{
    public class PlayerController : MonoBehaviour, IStateMachineProvider<PlayerActionState>
    {

        #region Public part
        public ActionStateMachine<PlayerActionState> StateMachine { get; private set; } = new ActionStateMachine<PlayerActionState>();
        #endregion

        #region Serialize part
        [Header("Movement")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private float _speed;
        [SerializeField] private float _jumpForce;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask _groundLayer;
        #endregion
        #region Private part
        private InputSystemActions inputActions;

        private Vector2 moveInput;
        private Vector2 lastMoveInput = new(1, 0);
        #endregion

        private void Awake()
        {
            inputActions = new InputSystemActions();
            _rb = _rb != null ? _rb : GetComponent<Rigidbody2D>();
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
            Move();
            UpdateState();
        }

        //private void Update()
        //{
            
        //}

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            UpdateDirection();
        }

        private void Move()
        {
            _rb.linearVelocity = new Vector2(moveInput.x * _speed, _rb.linearVelocity.y);
            
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (GroundCheck())
            {
                _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
            }
                
        }

        #region Support methods
        private void UpdateDirection()
        {
            if (moveInput.x != 0 && Math.Sign(moveInput.x) != Math.Sign(lastMoveInput.x))
            {
                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
                lastMoveInput = moveInput;
            }
        }

        private void UpdateState()
        {
            if (_rb.linearVelocityY > 0)
            {
                StateMachine.ChangeState(PlayerActionState.Jump);
            }
            else if (_rb.linearVelocityY < 0) StateMachine.ChangeState(PlayerActionState.Fall);
            else
            {
                if (_rb.linearVelocityX != 0) StateMachine.ChangeState(PlayerActionState.Run);
                else StateMachine.ChangeState(PlayerActionState.Idle);
            }
            
        }


        #endregion

        private bool GroundCheck()
        {
            if (Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer) != null) return true;
            else return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }

}
