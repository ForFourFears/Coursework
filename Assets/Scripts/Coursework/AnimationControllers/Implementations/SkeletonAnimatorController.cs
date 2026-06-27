using Coursework.AnimationControllers.Core;
using Coursework.EnumsCreatures.Skeleton;
using Coursework.LogicControllers.ActionStateMachines.Core;
using UnityEngine;

namespace Coursework.AnimationControllers.Implementations
{
    public class SkeletonAnimatorController : BaseAnimationController<SkeletonStates, SkeletonActions>
    {
        #region Animation Data

        #region States (Layer 0)
        private readonly AnimationData idle = new(Animator.StringToHash("Idle"), 0);
        private readonly AnimationData walk = new(Animator.StringToHash("Walk"), 0);
        private readonly AnimationData death = new(Animator.StringToHash("Death"), 0);
        #endregion

        #region Actions & Transitions (Layer 1)
        private readonly AnimationData attack = new(Animator.StringToHash("Attack"), 1);
        private readonly AnimationData hit = new(Animator.StringToHash("Hit"), 1);
        private readonly AnimationData deathTransition = new(Animator.StringToHash("DeathTransition"),1);
        private readonly AnimationData react = new(Animator.StringToHash("React"), 1);
        #endregion

        #endregion

        private readonly Rigidbody2D rigidbody;

        public SkeletonAnimatorController(
            Rigidbody2D rigidbody,
            Animator animator,
            IActionStateMachine<SkeletonStates, SkeletonActions> actionStateMachine,
            IObservableSMBsHandler animationEventsHandle
        ) : base(animator, actionStateMachine, animationEventsHandle)
        {
            this.rigidbody = rigidbody;
        }

        public override void Subscribe()
        {
            actionStateMachine[SkeletonActions.React].Action += OnActionReact;
            actionStateMachine[SkeletonStates.Attack].OnEnter += OnEnterAttackState;
            actionStateMachine[SkeletonActions.Hit].Action += OnActionHit;
            actionStateMachine[SkeletonStates.Death].OnEnter += OnEnterDeathState;
        }

        public override void Unsubscribe()
        {
            actionStateMachine[SkeletonActions.React].Action -= OnActionReact;
            actionStateMachine[SkeletonStates.Attack].OnEnter -= OnEnterAttackState;
            actionStateMachine[SkeletonActions.Hit].Action -= OnActionHit;
            actionStateMachine[SkeletonStates.Death].OnEnter -= OnEnterDeathState;
        }

        public void Update()
        {
            AnimationData targetAnim = actionStateMachine.CurrentState switch
            {
                SkeletonStates.Locomotion => Mathf.Abs(rigidbody.linearVelocityX) > 3 ? walk : idle,
                SkeletonStates.Death => death,
                _ => idle,
            };
            ChangeAnimation(targetAnim);
        }

        private void OnEnterAttackState(SkeletonStates context)
        {
            PlaySequence(attack);
        }

        private void OnEnterDeathState(SkeletonStates context)
        {
            PlaySequence(deathTransition);
        }

        private void OnActionReact()
        {
            PlaySequence(react);
        }

        private void OnActionHit()
        {
            PlaySequence(hit);
        }
    }
}
