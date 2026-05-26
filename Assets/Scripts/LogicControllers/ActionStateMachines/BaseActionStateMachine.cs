using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public abstract class BaseActionStateMachine<TState, TAction> : IActionStateMachine<TState>
        where TState : Enum
        where TAction : Enum
    {
        protected readonly Dictionary<TState, StateEvents> stateActions = new();
        public TState CurrentState { get; protected set; }
        protected StateEvents currentStateEvents;
        public IStateEvents this[TState state]
        {
            get
            {
                if (!stateActions.TryGetValue(state, out var stateAction))
                {
                    stateAction = new StateEvents();
                    stateActions.Add(state, stateAction);
                }
                return stateAction;
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
            
            if (stateActions.TryGetValue(CurrentState, out var newStateEvent))
            {
                newStateEvent.EnteredInvoke();
            }

            currentStateEvents = newStateEvent;
        }

        protected bool TryChangeState(TState newState)
        {
            ChangeState(newState);
            return true;
        }

        public abstract bool TryExecuteAction(TAction action);
    }
}

