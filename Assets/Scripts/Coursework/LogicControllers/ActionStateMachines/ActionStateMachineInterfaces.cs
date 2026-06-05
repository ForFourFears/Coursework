using System;

namespace Coursework.LogicControllers.ActionStateMachines
{


    public interface IActionStateMachine<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        TState CurrentState { get; }
        IStateEvents this[TState state] { get; }
        IActionEvent this[TAction action] { get; }
    }

    public interface IActionStateMachineProvider<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        public IActionStateMachine<TState, TAction> ActionStateMachine { get; }
    }
}