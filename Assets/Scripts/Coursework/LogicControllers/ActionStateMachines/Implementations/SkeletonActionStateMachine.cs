using Coursework.AnimationControllers.Core;
using Coursework.EnumsCreatures;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;


namespace Coursework.LogicControllers.ActionStateMachines.Implementations
{
    public class SkeletonActionStateMachine : BaseActionStateMachine<SkeletonStates, SkeletonActions>
    {
        public SkeletonActionStateMachine(
            ModifierSystem modifierSystem,
            IEntityDataHandler<SkeletonStates, SkeletonActions> entityDataHandler,
            IObservableSMBsHandler observableSMBsHandler)
            : base(modifierSystem, observableSMBsHandler, entityDataHandler)
        {

        }

        public override bool TryExecuteAction(SkeletonActions action)
        {
            return false;
        }
    }
}
