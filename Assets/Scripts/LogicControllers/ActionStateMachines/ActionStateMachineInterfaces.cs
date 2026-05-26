using System;

namespace Coursework.LogicControllers.ActionStateMachines
{
    #region StateMachineEnums
    public enum KnightActionState
    {
        None = 0,
        Locomotion,
        Air,
        Crouch,
        WallInteraction,
        TurnAround,
        Attack,
        Roll,
        Dash,
        Slide,
        Death,
        Hit 
    }
    #endregion

    public interface IStateEvents
    {
        event Action Entered;
        event Action Update;
        event Action Exit;
    }

    public interface IActionStateMachine<TState> where TState : Enum
    {
        TState CurrentState { get; }
        IStateEvents this[TState state] { get; }
    }

    public interface IStateMachineProvider<TState> where TState : Enum
    {
        public IActionStateMachine<TState> StateMachine { get; }
    }
}