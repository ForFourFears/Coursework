using System;
using UnityEngine;
using Coursework.Scripts.LogicController.ActionStateMachine;
using Coursework.Scripts.LogicController.ActionTriggerHub;

namespace Coursework.Scripts.Animation
{
    public class PlayerAnimatorController : BaseAnimationController<PlayerActionState, PlayerActionTrigger>
    {
        private void OnEnable()
        {
            stateMachine[PlayerActionState.Idle].Entered += OnIdle;
            stateMachine[PlayerActionState.Run].Entered += OnRun;
            stateMachine[PlayerActionState.Jump].Entered += OnJump;
            stateMachine[PlayerActionState.Fall].Entered += OnFall;
            stateMachine[PlayerActionState.Fall].Exit += OnFallEnd;
            stateMachine[PlayerActionState.TurnAround].Entered += OnTurnAround;
        }

        private void OnDisable()
        {
            stateMachine[PlayerActionState.Idle].Entered -= OnIdle;
            stateMachine[PlayerActionState.Run].Entered -= OnRun;
            stateMachine[PlayerActionState.Jump].Entered -= OnJump;
            stateMachine[PlayerActionState.Fall].Entered -= OnFall;
            stateMachine[PlayerActionState.Fall].Exit -= OnFallEnd;
            stateMachine[PlayerActionState.TurnAround].Entered -= OnTurnAround;
        }

        private void OnIdle()
        {
            _characterAnimator.SetBool("IsRun", false);
        }

        private void OnRun()
        {
            _characterAnimator.SetBool("IsRun", true);
        }

        private void OnJump()
        {
            _characterAnimator.SetTrigger("IsJump");
        }

        private void OnFall()
        {
            _characterAnimator.SetBool("IsFall", true);
        }

        private void OnFallEnd()
        {
            _characterAnimator.SetBool("IsFall", false);
        }

        private void OnTurnAround()
        {
            _characterAnimator.SetTrigger("IsTurnAround");
        }

        
    }
}