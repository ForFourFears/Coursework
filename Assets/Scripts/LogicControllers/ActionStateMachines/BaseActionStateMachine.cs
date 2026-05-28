using System;
using System.Collections.Generic;


namespace Coursework.LogicControllers.ActionStateMachines
{
    public abstract class BaseActionStateMachine<TState, TAction> : IActionStateMachine<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        protected readonly Dictionary<TState, StateEvents> stateEvents = new();
        protected readonly Dictionary<TAction, ActionEvent> actionEvents = new();
        public TState CurrentState { get; protected set; }
        protected StateEvents currentStateEvents;
        public IStateEvents this[TState state]
        {
            get
            {
                if (!stateEvents.TryGetValue(state, out var _stateEvents))
                {
                    _stateEvents = new StateEvents();
                    stateEvents.Add(state, _stateEvents);
                }
                return _stateEvents;
            }
        }

        public IActionEvent this[TAction action]
        {
            get
            {
                if (!actionEvents.TryGetValue(action, out var actionEvent))
                {
                    actionEvent = new ActionEvent();
                    actionEvents.Add(action, actionEvent);
                }
                return actionEvent;
            }
        }

        public virtual void Update()
        {
            currentStateEvents?.UpdateInvoke();
        }
        protected void ChangeState(TState newState)
        {
            if (Equals(CurrentState, newState)) return;

            currentStateEvents?.ExitInvoke();

            CurrentState = newState;
            
            if (stateEvents.TryGetValue(CurrentState, out var newStateEvent))
            {
                newStateEvent.EnteredInvoke();
            }

            currentStateEvents = newStateEvent;
        }
        public abstract bool TryExecuteAction(TAction action);

        protected bool TryChangeState(TState newState, TAction action)
        {
            ChangeState(newState);
            if (actionEvents.TryGetValue(action, out var actionEvent))
            {
                actionEvent.ActionInvoke();
                return true;
            }
            return false;            
        }
    }
}

