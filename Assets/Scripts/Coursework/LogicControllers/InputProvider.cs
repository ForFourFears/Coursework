using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionBuffers;
using Coursework.LogicControllers.CharactersControllers;
using Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Coursework.LogicControllers
{
    public class InputProvider : MonoBehaviour
    {
        #region Serialized Fields
        
        [SerializeField] private IKnightController  _controller;
        
        #endregion
        
        #region Private fields
        
        private InputSystemActions inputSystemActions;
        private ActionBuffer actionBuffer;
        
        private bool isInitialized;
        private bool isSubscribed;
        #endregion
        
        public void Initialize()
        {
            inputSystemActions = new();
            actionBuffer = new();
            
            _controller ??= GetComponent<IKnightController>();
            
            isInitialized = true;
            OnEnable();
        }
        
        private void OnEnable()
        {
            if (!isInitialized || isSubscribed) return;

            inputSystemActions.Enable();

            inputSystemActions.Player.Move.performed += OnMove;
            inputSystemActions.Player.Move.canceled += OnMove;

            inputSystemActions.Player.Crouch.performed += OnCrouch;
            inputSystemActions.Player.Crouch.canceled += OnCrouch;

            inputSystemActions.Player.Jump.performed += OnJump;

            //inputSystemActions.Player.Attack.performed += OnAttack;

            inputSystemActions.Player.Dash.performed += OnDash;

            //inputSystemActions.Player.Roll.performed += OnRoll;

            isSubscribed = true;
        }
        
        private void OnDisable()
        {
            if (!isInitialized || !isSubscribed) return;

            inputSystemActions.Player.Move.performed -= OnMove;
            inputSystemActions.Player.Move.canceled -= OnMove;

            inputSystemActions.Player.Crouch.performed -= OnCrouch;
            inputSystemActions.Player.Crouch.canceled -= OnCrouch;

            inputSystemActions.Player.Jump.performed -= OnJump;

            //inputSystemActions.Player.Attack.performed -= OnAttack;

            inputSystemActions.Player.Dash.performed -= OnDash;

            //inputSystemActions.Player.Roll.performed -= OnRoll;


            inputSystemActions.Disable();

            isSubscribed = false;
        }

        private void Update()
        {
            if (!isInitialized) return;

            actionBuffer.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;
            
            KnightActions actionType = KnightActions.None;
            if (inputSystemActions.Player.Roll.IsPressed()) actionType = KnightActions.Roll;
            else if (inputSystemActions.Player.Attack.IsPressed()) actionType = KnightActions.Attack;
 
            actionBuffer.AddAction(actionType, 0.2f);

            ActionRequest action = actionBuffer.GetOldestActionRequest();
            if(action.Action != KnightActions.None && _controller.TryExecuteAction(action.Action))
            {
                actionBuffer.RemoveAction(action);
            }
        }

        #region On Event Callbacks
        private void OnMove(InputAction.CallbackContext context)
        {
            _controller.MoveInput = context.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            KnightActions action = KnightActions.Jump;
            actionBuffer.AddAction(action, 0.2f);
        }

        private void OnCrouch(InputAction.CallbackContext context)
        {
            _controller.IsCrouched = context.ReadValueAsButton();
        }

        //private void OnAttack(InputAction.CallbackContext context)
        //{
        //    KnightActions actionAttack = KnightActions.Attack;
        //    actionBuffer.AddAction(actionAttack, 0.2f);
        //}

        private void OnDash(InputAction.CallbackContext context)
        {
            KnightActions actionAttack = KnightActions.Dash;
            actionBuffer.AddAction(actionAttack, 0.2f);
        }

        //private void OnRoll(InputAction.CallbackContext context)
        //{
        //    KnightActions actionAttack = KnightActions.Roll;
        //    actionBuffer.AddAction(actionAttack, 0.2f);
        //}
        #endregion
    }
}