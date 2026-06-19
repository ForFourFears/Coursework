using System;


namespace Coursework.LogicControllers.ActionStateMachines
{
    public interface IStateEvents<TState> where TState : Enum
    {
        event Action <TState> OnEnter;
        event Action OnUpdate;
        event Action <TState> OnExit;
    }
    public class StateEvents<TState> : IStateEvents<TState> where TState : Enum
    {
        public event Action<TState> OnEnter;
        public event Action OnUpdate;
        public event Action<TState> OnExit;

        public void EnteredInvoke(TState previousState) => OnEnter?.Invoke(previousState);
        public void UpdateInvoke() => OnUpdate?.Invoke();
        public void ExitInvoke(TState nextState) => OnExit?.Invoke(nextState);
    }
}