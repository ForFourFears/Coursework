using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class ActionStateMachine<TState> : IActionStateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, StateTrigger> stateTriggers = new();
        public TState CurrentState { get; private set; }
        public IStateTrigger this[TState state]
        {
            get
            {
                if (!stateTriggers.TryGetValue(state, out var stateTrigger))
                {
                    stateTrigger = new StateTrigger();
                    stateTriggers.Add(state, stateTrigger);
                }
                return stateTrigger;
            }
        }
        public void ChangeState(TState newState)
        {
            if (Equals(CurrentState, newState)) return;

            if (stateTriggers.TryGetValue(CurrentState, out var currentStateEvent))
            {
                currentStateEvent.ExitInvoke();
            }
            CurrentState = newState;

            if (stateTriggers.TryGetValue(CurrentState, out var newStateEvent))
            {
                newStateEvent.EnteredInvoke();
            }
        }
    }
}

