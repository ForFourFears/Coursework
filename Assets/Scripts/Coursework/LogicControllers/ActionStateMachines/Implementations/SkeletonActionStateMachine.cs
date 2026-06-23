using Coursework.AnimationControllers.Core;
using Coursework.EnumsCreatures.Knight;
using Coursework.EnumsCreatures.Skeleton;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;


namespace Coursework.LogicControllers.ActionStateMachines.Implementations
{
    public class SkeletonActionStateMachine : BaseActionStateMachine<SkeletonStates, SkeletonActions>
    {
        private readonly IMovementContext movementContext;
        private readonly Rigidbody2D rigidbody;
        private readonly IBaseEntityContext entityContext;

        private static readonly HashSet<SkeletonStates> NonInterruptibleStates = new()
        {
            SkeletonStates.Attack,
            SkeletonStates.Death
        };

        public SkeletonActionStateMachine(
            IMovementContext movementContext,
            IBaseEntityContext entityContext,
            ModifierSystem modifierSystem,
            IEntityDataHandler<SkeletonStates, SkeletonActions> entityDataHandler,
            IObservableSMBsHandler observableSMBsHandler)
            : base(modifierSystem, observableSMBsHandler, entityDataHandler)
        {
            this.movementContext = movementContext;
            rigidbody = movementContext.Rigidbody;
            this.entityContext = entityContext;

            CurrentState = SkeletonStates.None;
        }

        public void Subscribe()
        {
            observableSMBsHandler["Attack"].ExitState += CheckTransitions;
        }

        public void Unsubscribe()
        {
            observableSMBsHandler["Attack"].ExitState -= CheckTransitions;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if (!NonInterruptibleStates.Contains(CurrentState)) CheckTransitions();

            UpdateConstraints();
        }

        public void CheckTransitions()
        {
            ChangeState(SkeletonStates.Locomotion);
        }

        private void UpdateConstraints()
        {
            if (entityContext.IsGrounded && movementContext.MoveInput.x == 0 &&
                movementContext.SlopeAngle <= movementContext.MaxSlopeAngle &&
                CurrentState == SkeletonStates.Locomotion
            )
            {
                rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                return;
            }
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public override bool TryExecuteAction(SkeletonActions action)
        {
            return CurrentState switch
            {
                SkeletonStates.Locomotion => action switch
                {
                    SkeletonActions.Attack => TryChangeState(SkeletonStates.Attack, action),
                    SkeletonActions.Hit => TryTriggerAction(action),
                    SkeletonActions.React => TryTriggerAction(action),
                    _ => false
                },
                SkeletonStates.Attack => action switch 
                {
                    SkeletonActions.Hit => TryTriggerAction(action),
                    _ => false
                },
                _ => false,
            };
        }
    }
}
