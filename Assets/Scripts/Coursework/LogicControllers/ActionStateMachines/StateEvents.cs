using System;


namespace Coursework.LogicControllers.ActionStateMachines
{
    public interface IStateEvents
    {
        event Action Entered;
        event Action Update;
        event Action Exit;
    }
    public class StateEvents : IStateEvents
    {
        public event Action Entered;
        public event Action Update;
        public event Action Exit;

        public void EnteredInvoke() => Entered?.Invoke();
        public void UpdateInvoke() => Update?.Invoke();
        public void ExitInvoke() => Exit?.Invoke();
    }
}