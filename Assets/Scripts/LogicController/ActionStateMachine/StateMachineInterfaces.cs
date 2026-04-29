using System;

namespace Coursework.Scripts.LogicController.ActionStateMachine
{
    #region StateMachineEnums
    public enum PlayerActionState
    {
        Idle,
        Run,
        Jump,
        Fall,
        TurnAround
    }
    #endregion

    public interface IStateTrigger
    {
        event Action Entered;
        event Action Exit;
    }

    public interface IActionStateMachine<TState> where TState : Enum
    {
        TState CurrentState { get; }
        IStateTrigger this[TState state] { get; }
    }

    public interface IStateMachineProvider<TState> where TState : Enum
    {
        public IActionStateMachine<TState> StateMachine { get; }
    }
}