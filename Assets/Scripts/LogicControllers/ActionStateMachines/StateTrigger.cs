using System;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class StateTrigger : IStateTrigger
    {
        public event Action Entered;
        public event Action Exit;

        public void EnteredInvoke() => Entered?.Invoke();
        public void ExitInvoke() => Exit?.Invoke();
    }
}