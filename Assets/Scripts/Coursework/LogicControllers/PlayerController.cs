using System;
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
using Coursework.LogicControllers.AttackSystems;

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
        public Vector2 SlopeDirection { get; }
        public Rigidbody2D Rigidbody { get; }
    }

    public interface IDamageable
    {
        public void TakeDamage(float damage);
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IEntityContext, IMovementContext, IAttacker, IDamageable/*, IActionStateMachineProvider<KnightActionStates,  KnightActions>*/
    {
        #region Public part
        public bool IsGrounded { get; private set; }
        public bool IsCrouched { get; private set; }
        public bool IsCeilingAbove { get; private set; }
        public bool IsAttacking { get; private set; }
        public bool IsRolling { get; private set; }
        public bool IsDashing { get; private set; }

        public Vector2 MoveInput { get; private set; }
        public Vector2 SlopeDirection { get; private set; }

        public IActionStateMachine<KnightActionStates, KnightActions> ActionStateMachine { get => actionStateMachine; }
        #endregion

        #region Serialize part
        [Header("Movement")]
        [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }
        [SerializeField] private PhysicsMaterial2D _fricrion0;
        [SerializeField] private PhysicsMaterial2D _fricrion1;

        [Header("Slope Detection")]
        [SerializeField] private float _maxSlopeAngle;
        [SerializeField] private Transform _normalOrigin;
        [SerializeField] private float _normalVectorLength = 0.3f;

        [Header("Grounded Check")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Ceiling Check")]
        [SerializeField] private Transform _ceilingCheck;
        [SerializeField] private float _ceilingCheckRadius = 0.1f;
        [SerializeField] private LayerMask _ceilingLayer;

        [Header("Coyote Time")]
        [SerializeField] private float _coyoteTimeDuration = 0.15f;

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

        private float coyoteTimeCounter;
        #endregion

        private void Awake()
        {
            Rigidbody = Rigidbody != null ? Rigidbody : GetComponent<Rigidbody2D>();

            inputSystemActions = new();
            actionBuffer = new();
            modifierSystem = new();
            movementSystem = new(this, this, modifierSystem);
            coyoteTimeCounter = _coyoteTimeDuration;

            _animator = _animator != null ? _animator : GetComponent<Animator>();

            observableSMBsHandler = new(_animator);
            actionStateMachine = new(this, this, modifierSystem, _knightConfig, observableSMBsHandler);
            actionExecutionSystem = new(this, ActionStateMachine, _knightConfig);

            animatorController = new (this, Rigidbody, _animator, ActionStateMachine, observableSMBsHandler);

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

            UpdateFriction();
            actionStateMachine.Update(Time.fixedDeltaTime);

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

        private void UpdateFriction()
        {
            if (IsGrounded && MoveInput.x == 0)
            {
                Rigidbody.sharedMaterial = _fricrion1;
            }
            else
            {
                Rigidbody.sharedMaterial = _fricrion0;
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

            bool isGround = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

            СalculateSlopeDirection(new Vector2(1, 0));

            return isGround;
        }

        private void СalculateSlopeDirection(Vector2 deafult)
        {
            RaycastHit2D hit = Physics2D.Raycast(_normalOrigin.position, Vector2.down, _normalVectorLength, _groundLayer);

            if (hit.normal == Vector2.zero)
            {
                SlopeDirection = deafult;
                return;
            }

            float slopeAngle = Vector2.Angle(Vector2.up, hit.normal);

            if (slopeAngle <= _maxSlopeAngle)
            {
                SlopeDirection = new Vector2(hit.normal.y, -hit.normal.x);
                return;
            }

            SlopeDirection = deafult;
            return;
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
                GUIStyle labelStyle = new()
                {
                    fontSize = 32
                };
                labelStyle.normal.textColor = Color.white; 
                labelStyle.alignment = TextAnchor.MiddleCenter; 


                Handles.Label(infoPosition.position, 
                    $"{actionStateMachine.CurrentState}, {modifierSystem.StateModifier},\n" +
                    $" SlopeDirection: {SlopeDirection}\n" +
                    $"IsGrounded: {IsGrounded}", labelStyle);
            }

            if (SlopeDirection != Vector2.zero)
            {
                Vector3 startPoint = SlopeDirectionPosition.position;
                Vector3 endPoint = startPoint + (Vector3)SlopeDirection * 1f;
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
                Vector3 endPoint = startPoint + Vector3.down * _normalVectorLength;
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