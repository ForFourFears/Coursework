using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ModifierSystems;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class KnightActionStateMachine : BaseActionStateMachine<KnightActionStates, KnightActions>
    {
        private readonly IEntityContext entityContext;
        private readonly Rigidbody2D rigidbody2D;
        private readonly ModifierSystem modifierSystem;
        public KnightActionStateMachine(IEntityContext entityContext, IMovementContext movementContext, ModifierSystem modifierSystem)
        {
            this.entityContext = entityContext;
            rigidbody2D = movementContext.Rigidbody;
            this.modifierSystem = modifierSystem;
        }

        public override void Update()
        {
            base.Update();
            CheckTransitions();

        }

        public override bool TryExecuteAction(KnightActions action)
        {
            return CurrentState switch
            {
                KnightActionStates.Locomotion => action switch
                {
                    KnightActions.Jump => CanJump(KnightActionStates.Air, action),
                    KnightActions.Crouch => TryChangeState(KnightActionStates.Crouch, action),
                    _ => false
                },
                KnightActionStates.Air => action switch
                {
                    KnightActions.Jump => CanJump(KnightActionStates.Air, action),
                    _ => false
                },
                KnightActionStates.Crouch => action switch
                {
                    KnightActions.Jump => CanJump(KnightActionStates.Air, action),
                    _ => false
                },
                _ => false
            };
        }

        private bool CanJump(KnightActionStates state, KnightActions action)
        {
            if (entityContext.IsGrounded)
            {
                return TryChangeState(KnightActionStates.Air, action);
            }
            else return false;
        }
        private void CheckTransitions()
        {
            if (!entityContext.IsGrounded)
            {
                ChangeState(KnightActionStates.Air);
            }
            else if (rigidbody2D.linearVelocityY <= 0)
            {
                if (entityContext.IsCrouchIntentHeld || entityContext.IsCeilingAbove)
                {
                    ChangeState(KnightActionStates.Crouch);
                }
                else
                {
                    ChangeState(KnightActionStates.Locomotion);
                }
            }
        }
    }
}
