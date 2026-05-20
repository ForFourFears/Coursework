using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public abstract class BaseActionStateMachine<TState> : IActionStateMachine<TState> where TState : Enum
    {
        protected readonly Dictionary<TState, StateActions> stateActions = new();
        public TState CurrentState { get; protected set; }
        protected StateActions currentAction;
        public IStateActions this[TState state]
        {
            get
            {
                if (!stateActions.TryGetValue(state, out var stateTrigger))
                {
                    stateTrigger = new StateActions();
                    stateActions.Add(state, stateTrigger);
                }
                return stateTrigger;
            }
        }

        public void Update()
        {
            currentAction?.UpdateInvoke();
        }
        protected void ChangeState(TState newState)
        {
            if (Equals(CurrentState, newState)) return;

            if (stateActions.TryGetValue(CurrentState, out var currentStateEvent))
            {
                currentStateEvent.ExitInvoke();
            }
            CurrentState = newState;
            
            if (stateActions.TryGetValue(CurrentState, out var newStateEvent))
            {
                newStateEvent.EnteredInvoke();
            }

            currentAction = newStateEvent;
        }

        public abstract void TryExecuteAction(Action action);
    }
}

