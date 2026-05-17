using System;
using UnityEngine;
using Coursework.LogicControllers.ActionTriggerHubs;
using Coursework.LogicControllers.ActionStateMachines;

namespace Coursework.AnimationControllers
{
    public class KnightAnimatorController : BaseAnimationController<PlayerActionState, PlayerActionTrigger>
    {
        #region Animator Hashes
        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int RunHash = Animator.StringToHash("Run");

        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int HitHash = Animator.StringToHash("Hit");

        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int JumpFallInBetweenHash = Animator.StringToHash("JumpFallInBetween");
        private static readonly int FallHash = Animator.StringToHash("Fall");

        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
        private static readonly int AttackComboHash = Animator.StringToHash("AttackCombo");

        private static readonly int WallHangHash = Animator.StringToHash("WallHang");
        private static readonly int WallClimbHash = Animator.StringToHash("WallClimb");
        private static readonly int WallSlideHash = Animator.StringToHash("WallSlide");

        private static readonly int CrouchHash = Animator.StringToHash("Crouch");
        private static readonly int CrouchAttackHash = Animator.StringToHash("CrouchAttack");
        private static readonly int CrouchTransitionHash = Animator.StringToHash("CrouchTransition");
        private static readonly int CrouchWalkHash = Animator.StringToHash("CrouchWalk");
        private static readonly int CrouchFullHash = Animator.StringToHash("CrouchFull");

        private static readonly int DashHash = Animator.StringToHash("Dash");
        private static readonly int RollHash = Animator.StringToHash("Roll");

        private static readonly int SlideTransitionStartHash = Animator.StringToHash("SlideTransitionStart");
        private static readonly int SlideHash = Animator.StringToHash("Slide");
        private static readonly int SlideTransitionEndHash = Animator.StringToHash("SlideTransitionEnd");
        private static readonly int SlideFullHash = Animator.StringToHash("SlideFull");
        #endregion

        [SerializeField] private float _airStateThreshold = 1f;

        private bool isBlockedUpdate;

        private void OnEnable()
        {
            stateMachine[PlayerActionState.Crouch].Entered += OnEnteredCrounchState;
            stateMachine[PlayerActionState.Crouch].Exit += OnExitCrounchState;
            animationEventHub[CrouchTransitionHash].ExitState += OnCrouchTransitionEnd;
        }

        private void OnDisable()
        {
            stateMachine[PlayerActionState.Crouch].Entered -= OnEnteredCrounchState;
            stateMachine[PlayerActionState.Crouch].Exit -= OnExitCrounchState;
            animationEventHub[CrouchTransitionHash].ExitState -= OnCrouchTransitionEnd;
        }

        private void Update()
        {
            if (isBlockedUpdate) return;
            switch (stateMachine.CurrentState)
            {
                case PlayerActionState.Locomotion: OnLocomotion(); break;
                case PlayerActionState.Air: OnAir(); break;
                case PlayerActionState.Crouch: OnCrouch(); break;
                default: Debug.LogError($"[KnightAnimatorController]: unidentified State: {stateMachine.CurrentState}"); break;
            }
        }

        private void OnEnteredCrounchState()
        {
            isBlockedUpdate = true;
            ChangeAnimation(CrouchTransitionHash, 0);
        }

        private void OnExitCrounchState()
        {
            isBlockedUpdate = true;
            ChangeAnimation(CrouchTransitionHash, 0);
        }

        private void OnCrouchTransitionEnd()
        {
            isBlockedUpdate = false;
        }

        #region On Player Action State
        private void OnLocomotion()
        {
            int targetAnimationHash = Mathf.Abs(rb.linearVelocityX) switch
            {
                > 0.1f => RunHash,
                _ => IdleHash,
            };
            ChangeAnimation(targetAnimationHash, 0);
        }


        private void OnAir()
        {
            int targetAnimationHash = rb.linearVelocityY switch
            {
                var y when y > _airStateThreshold => JumpHash,
                var y when y < -_airStateThreshold => FallHash,
                _ => JumpFallInBetweenHash,
            };
            ChangeAnimation(targetAnimationHash, 0);
        }

        private void OnCrouch()
        {
            int targetAnimationHash = Mathf.Abs(rb.linearVelocityX) switch
            {
                > 0.1f => CrouchWalkHash,
                _ => CrouchHash,
            };
            ChangeAnimation(targetAnimationHash, 0);
        }

        private void OnWallInteraction()
        {

        }

        private void OnTurnAround()
        {
            
        }

        private void OnAttack()
        {

        }

        private void OnRoll()
        {

        }

        private void OnDash()
        {

        }

        private void OnSlide()
        {

        }

        private void OnDeath()
        {

        }

        private void OnHit()
        {

        }
        #endregion
    }
}