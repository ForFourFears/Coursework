using System;

namespace Coursework.LogicControllers.ActionStateMachines.Core
{


    public interface IActionStateMachine<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        TState CurrentState { get; }
        IStateEvents<TState> this[TState state] { get; }
        IActionEvent this[TAction action] { get; }
    }

    public interface IActionStateMachineProvider<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        public IActionStateMachine<TState, TAction> ActionStateMachine { get; }
    }
}