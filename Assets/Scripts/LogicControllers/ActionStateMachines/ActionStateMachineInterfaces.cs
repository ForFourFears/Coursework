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

    //public interface IStateMachineProvider<TState> where TState : Enum
    //{
    //    public IActionStateMachine<TState> StateMachine { get; }
    //}
}