using System;

namespace Coursework.Scripts.LogicController
{
    #region StateMachineEnums
    public enum PlayerActionState
    {
        Idle,
        Run,
        Jump,
        Fall
    }
    #endregion

    public interface IStateEvents
    {
        event Action Entered;
        event Action Exit;
    }

    internal class StateTrigger : IStateEvents
    {
        public event Action Entered;
        public event Action Exit;

        public void EnteredInvoke() => Entered?.Invoke();
        public void ExitInvoke() => Exit?.Invoke();
    }

    public interface IActionStateMachine<TState> where TState : Enum
    {
        TState CurrientState { get; }
        IStateEvents this[TState state] { get; }
    }

    public interface IStateMachineProvider<TState> where TState : Enum
    {
        ActionStateMachine<TState> StateMachine { get; }
    }
}