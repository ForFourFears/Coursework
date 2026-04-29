using System;

namespace Coursework.Scripts.LogicController.ActionStateMachine
{
    public class StateTrigger : IStateTrigger
    {
        public event Action Entered;
        public event Action Exit;

        public void EnteredInvoke() => Entered?.Invoke();
        public void ExitInvoke() => Exit?.Invoke();
    }
}