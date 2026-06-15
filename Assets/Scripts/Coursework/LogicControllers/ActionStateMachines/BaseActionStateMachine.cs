using Coursework.AnimationControllers;
using System;
using System.Collections.Generic;


namespace Coursework.LogicControllers.ActionStateMachines
{
    public abstract class BaseActionStateMachine<TState, TAction> : IActionStateMachine<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        protected readonly Dictionary<TState, StateEvents<TState>> stateEvents;
        protected readonly Dictionary<TAction, ActionEvent> actionEvents;
        protected readonly IObservableSMBsHandler observableSMBsHandler;

        public BaseActionStateMachine(IObservableSMBsHandler observableSMBsHandler)
        {
            this.observableSMBsHandler = observableSMBsHandler;
            stateEvents = new();
            actionEvents = new();
        }

        public TState CurrentState { get; protected set; }
        protected StateEvents<TState> currentStateEvents;
        public IStateEvents<TState> this[TState state]
        {
            get
            {
                if (!stateEvents.TryGetValue(state, out var _stateEvents))
                {
                    _stateEvents = new StateEvents<TState>();
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

        public virtual void Update(float deltaTime)
        {
            currentStateEvents?.UpdateInvoke();
        }
        protected void ChangeState(TState newState)
        {
            if (Equals(CurrentState, newState)) return;

            currentStateEvents?.ExitInvoke(newState);

            TState previousState = CurrentState;
            CurrentState = newState;

            OnStateChanged(CurrentState);

            if (stateEvents.TryGetValue(CurrentState, out var newStateEvent))
            {
                newStateEvent.EnteredInvoke(previousState);
            }

            currentStateEvents = newStateEvent;
        }

        protected virtual void OnStateChanged(TState currentState) { }

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

