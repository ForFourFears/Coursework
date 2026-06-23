using Coursework.AnimationControllers.Core;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers;
using Coursework.LogicControllers.ActionStateMachines.Core;
using UnityEngine;

namespace Coursework.AnimationControllers.Implementations
{
    public class KnightAnimatorController : BaseAnimationController<KnightStates, KnightActions>
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
        private static readonly AnimationData dashL0 = new(Animator.StringToHash("DashL0"), 0);
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
        private static readonly AnimationData dashL1 = new(Animator.StringToHash("DashL1"), 1);
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
            IActionStateMachine<KnightStates, KnightActions> actionStateMachine, 
            IObservableSMBsHandler animationEventsHandle) 
            : base (rigidbody, animator, actionStateMachine, animationEventsHandle) 
        { 
            this.entityContext = entityContext;
        }

        public override void Subscribe()
        {
            actionStateMachine[KnightStates.Crouch].OnEnter += OnCrouchStateTransition;
            actionStateMachine[KnightStates.Crouch].OnExit += OnCrouchStateTransition;

            actionStateMachine[KnightStates.Attack].OnEnter += OnEnterAttackState;
            actionStateMachine[KnightStates.Attack2].OnEnter += OnEnterAttack2State;
            actionStateMachine[KnightStates.CrouchAttack].OnEnter += OnEnterCrouchAttackState;

            actionStateMachine[KnightStates.Attack].OnExit += OnActionInterrupted;
            actionStateMachine[KnightStates.Attack2].OnExit += OnActionInterrupted;
            actionStateMachine[KnightStates.CrouchAttack].OnExit += OnActionInterrupted;
        }

        public override void Unsubscribe()
        {
            actionStateMachine[KnightStates.Crouch].OnEnter -= OnCrouchStateTransition;
            actionStateMachine[KnightStates.Crouch].OnExit -= OnCrouchStateTransition;

            actionStateMachine[KnightStates.Attack].OnEnter -= OnEnterAttackState;
            actionStateMachine[KnightStates.Attack2].OnEnter -= OnEnterAttack2State;
            actionStateMachine[KnightStates.CrouchAttack].OnEnter -= OnEnterCrouchAttackState;

            actionStateMachine[KnightStates.Attack].OnExit -= OnActionInterrupted;
            actionStateMachine[KnightStates.Attack2].OnExit -= OnActionInterrupted;
            actionStateMachine[KnightStates.CrouchAttack].OnExit -= OnActionInterrupted;
        }

        public void Update()
        {
            AnimationData targetAnim;
            switch (actionStateMachine.CurrentState)
            {
                case KnightStates.Locomotion:
                    targetAnim = Mathf.Abs(rb.linearVelocityX) > 5 ? run : idle;
                    break;

                case KnightStates.Air:
                    if (rb.linearVelocityY < 0 && currentLayerHashes[0] != fall.Hash) PlaySequence(jumpFallInBetween);
                    targetAnim = rb.linearVelocityY >= 0 ? jump : fall;
                    break;

                case KnightStates.Crouch:
                    targetAnim = Mathf.Abs(rb.linearVelocityX) > 1 ? crouchWalk : crouch;
                    break;

                case KnightStates.Attack or KnightStates.Attack2:
                    targetAnim = idle;
                    break;

                case KnightStates.CrouchAttack:
                    targetAnim = crouch;
                    break;

                case KnightStates.Dash:
                    targetAnim = dashL0;
                    break;

                default:
                    targetAnim = Mathf.Abs(rb.linearVelocityX) > 5 ? run : idle;
                    break;
            }
            ChangeAnimation(targetAnim);
        }

        private void OnCrouchStateTransition(KnightStates state)
        {
            if (state == KnightStates.CrouchAttack) return;
            PlaySequence(crouchTransition);
        }


        private void OnActionInterrupted(KnightStates context)
        {
            ChangeAnimation(entryL1);
        }

        private void OnEnterAttackState(KnightStates context)
        {
            PlaySequence(attack);
        }

        private void OnEnterAttack2State(KnightStates context)
        {
            PlaySequence(attack2);
        }

        private void OnEnterCrouchAttackState(KnightStates context)
        {
            PlaySequence(crouchAttack);
        }

    }
}