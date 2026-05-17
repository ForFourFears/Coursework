using System;

namespace Coursework.LogicControllers.ActionStateMachines
{
    #region StateMachineEnums
    public enum PlayerActionState
    {
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