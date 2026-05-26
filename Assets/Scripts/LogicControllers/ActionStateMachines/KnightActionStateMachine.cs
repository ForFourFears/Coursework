using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ModifierSystems;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class KnightActionStateMachine : BaseActionStateMachine<KnightActionState, KnightActions>
    {
        private readonly IEntityContext entityContext;
        public KnightActionStateMachine(IEntityContext entityContext, ModifierSystem modifierSystems)
        {
            this.entityContext = entityContext;
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
                KnightActionState.Locomotion => action switch
                {
                    KnightActions.Jump => TryChangeState(KnightActionState.Air),
                    KnightActions.Crouch => TryChangeState(KnightActionState.Crouch),
                    _ => false 
                },
                KnightActionState.Air => action switch
                { 
                    KnightActions.Jump => TryChangeState(KnightActionState.Air),
                    _ => false
                },
                KnightActionState.Crouch => action switch
                {
                    KnightActions.Jump => TryChangeState(KnightActionState.Air),
                    _ => false
                },
                _ => false
            };
        }

        private void CheckTransitions()
        {
            if (!entityContext.IsGrounded)
            {
                ChangeState(KnightActionState.Air);
            }
            else
            {
                if (entityContext.IsCrouchIntentHeld || entityContext.IsCeilingAbove)
                {
                    ChangeState(KnightActionState.Crouch);
                }
                else
                {
                    ChangeState(KnightActionState.Locomotion);
                }
            }

        }
    }
}
