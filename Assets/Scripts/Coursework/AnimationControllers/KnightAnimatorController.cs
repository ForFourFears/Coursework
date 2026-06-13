using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers;
using Coursework.LogicControllers.ActionStateMachines;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Coursework.AnimationControllers
{
    public class KnightAnimatorController : BaseAnimationController<KnightActionStates, KnightActions>
    {
        #region Animation Data
        #region States (Layer 0)
        private static readonly AnimationData idle = new(Animator.StringToHash("Idle"), 0);
        private static readonly AnimationData run = new(Animator.StringToHash("Run"), 0);
        private static readonly AnimationData jump = new(Animator.StringToHash("Jump"), 0);
        private static readonly AnimationData fall = new(Animator.StringToHash("Fall"), 0);
        private static readonly AnimationData death = new(Animator.StringToHash("Death"), 0);
        private static readonly AnimationData wallHang = new(Animator.StringToHash("WallHang"), 0);
        private static readonly AnimationData wallClimb = new(Animator.StringToHash("WallClimb"), 0);
        private static readonly AnimationData wallSlide = new(Animator.StringToHash("WallSlide"), 0);
        private static readonly AnimationData crouch = new(Animator.StringToHash("Crouch"), 0);
        private static readonly AnimationData crouchWalk = new(Animator.StringToHash("CrouchWalk"), 0);
        private static readonly AnimationData crouchFull = new(Animator.StringToHash("CrouchFull"), 0); //???
        private static readonly AnimationData roll = new(Animator.StringToHash("Roll"), 0);
        private static readonly AnimationData slide = new(Animator.StringToHash("Slide"), 0);
        private static readonly AnimationData slideFull = new(Animator.StringToHash("SlideFull"), 0); //???

        #endregion
        #region Actions & Transitions (Layer 1)
        private static readonly AnimationData hit = new(Animator.StringToHash("Hit"), 1);
        private static readonly AnimationData attack = new(Animator.StringToHash("Attack"), 1);
        private static readonly AnimationData attack2 = new(Animator.StringToHash("Attack2"), 1);
        private static readonly AnimationData attackCombo = new(Animator.StringToHash("AttackCombo"), 1); //???
        private static readonly AnimationData crouchAttack = new(Animator.StringToHash("CrouchAttack"), 1);
        private static readonly AnimationData jumpFallInBetween = new(Animator.StringToHash("JumpFallInBetween"), 1);
        private static readonly AnimationData crouchTransition = new(Animator.StringToHash("CrouchTransition"), 1);
        private static readonly AnimationData dash = new(Animator.StringToHash("Dash"), 1);
        private static readonly AnimationData slideTransitionStart = new(Animator.StringToHash("SlideTransitionStart"), 1);
        private static readonly AnimationData slideTransitionEnd = new(Animator.StringToHash("SlideTransitionEnd"), 1);
        #endregion
        #endregion

        //[SerializeField] private float _airStateThreshold = 1f;
        private readonly IEntityContext entityContext;

        public KnightAnimatorController (
            IEntityContext entityContext,
            Rigidbody2D rigidbody,
            Animator animator,  
            IActionStateMachine<KnightActionStates, KnightActions> actionStateMachine, 
            IObservableSMBsHandler animationEventsHandle) 
            : base (rigidbody, animator, actionStateMachine, animationEventsHandle) 
        { 
            this.entityContext = entityContext;
        }

        public override void Subscribe()
        {
            actionStateMachine[KnightActionStates.Crouch].Entered += OnCrouchStateEntered;
            actionStateMachine[KnightActionStates.Crouch].Exit += OnCrouchStateExited;

            actionStateMachine[KnightActions.Attack].Action += OnActionAttack;
            actionStateMachine[KnightActionStates.Attack].Exit += OnActionInterrupted;
            actionStateMachine[KnightActionStates.CrouchAttack].Exit += OnActionInterrupted;
        }

        public override void Unsubscribe()
        {
            actionStateMachine[KnightActionStates.Crouch].Entered -= OnCrouchStateEntered;
            actionStateMachine[KnightActionStates.Crouch].Exit -= OnCrouchStateExited;

            actionStateMachine[KnightActions.Attack].Action -= OnActionAttack;
            actionStateMachine[KnightActionStates.Attack].Exit -= OnActionInterrupted;
            actionStateMachine[KnightActionStates.CrouchAttack].Exit -= OnActionInterrupted;
        }

        public void Update()
        {
            AnimationData targetAnim;
            switch (actionStateMachine.CurrentState)
            {
                case KnightActionStates.Locomotion or KnightActionStates.Attack:
                    targetAnim = Mathf.Abs(rb.linearVelocityX) > 0.5 ? run : idle;
                    break;
                case KnightActionStates.Air:
                    if (rb.linearVelocityY < 0 && currentLayerHashes[0] == jump.Hash) PlaySequence(jumpFallInBetween);
                    targetAnim = rb.linearVelocityY >= 0 ? jump : fall;
                    break;
                case KnightActionStates.Crouch or KnightActionStates.CrouchAttack:
                    targetAnim = Mathf.Abs(rb.linearVelocityX) > 0.5 ? crouchWalk : crouch;
                    break;
                default:
                    targetAnim = Mathf.Abs(rb.linearVelocityX) > 0.5 ? run : idle;
                    break;
            }
            ChangeAnimation(targetAnim);
        }

        private void OnCrouchStateEntered(KnightActionStates previousState)
        {
            if (previousState == KnightActionStates.CrouchAttack) return;
            PlaySequence(crouchTransition);
        }

        private void OnCrouchStateExited(KnightActionStates nextState)
        {
            if (nextState == KnightActionStates.CrouchAttack) return;
            PlaySequence(crouchTransition);
        }

        private void OnActionInterrupted(KnightActionStates context)
        {
            ChangeAnimation(entryL1);
        }
        
        private void OnActionAttack()
        {
            if (!entityContext.IsCrouched && !entityContext.IsCeilingAbove) PlaySequence(attack);
            else PlaySequence(crouchAttack);
        }
    }
}