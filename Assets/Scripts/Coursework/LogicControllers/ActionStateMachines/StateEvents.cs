using System;


namespace Coursework.LogicControllers.ActionStateMachines
{
    public interface IStateEvents<TState> where TState : Enum
    {
        event Action <TState> Entered;
        event Action Update;
        event Action <TState> Exit;
    }
    public class StateEvents<TState> : IStateEvents<TState> where TState : Enum
    {
        public event Action<TState> Entered;
        public event Action Update;
        public event Action<TState> Exit;

        public void EnteredInvoke(TState previousState) => Entered?.Invoke(previousState);
        public void UpdateInvoke() => Update?.Invoke();
        public void ExitInvoke(TState nextState) => Exit?.Invoke(nextState);
    }
}