using UnityEngine;
using UnityEngine.InputSystem;


namespace Scripts
{
    public class CharacterController : MonoBehaviour
    {
        private InputSystemActions inputActions;

        [Header("Movement")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _speed;
        [SerializeField] private float _jumpForce;

        private Vector2 moveInput;

        private void Awake()
        {
            inputActions = new InputSystemActions();
            _rb = _rb != null ? _rb : GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            inputActions.Enable();

            inputActions.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += context => moveInput = Vector2.zero;

            inputActions.Player.Jump.performed += OnJumpPerformed;
        }

        private void OnDisable()
        {
            inputActions.Player.Move.performed -= context => moveInput = context.ReadValue<Vector2>();
            inputActions.Player.Move.canceled -= context => moveInput = Vector2.zero;

            inputActions.Player.Jump.performed -= OnJumpPerformed;

            inputActions.Disable();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            _rb.linearVelocity = new Vector2(moveInput.x * _speed, _rb.linearVelocity.y);
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (GroundCheck()) _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
        }

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
