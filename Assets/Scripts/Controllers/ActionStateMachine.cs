using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.Controller
{
    public class ActionStateMachine<TState> : IActionStateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, StateTrigger> _stateTriggers = new();
        public TState CurrientState { get; private set; }
        public IStateEvents this[TState state]
        {
            get
            {
                if (!_stateTriggers.TryGetValue(state, out var trigger))
                {
                    trigger = new StateTrigger();
                    _stateTriggers.Add(state, trigger);
                }
                return trigger;
            }
        }
        public void ChangeState(TState newState)
        {
            if (Equals(CurrientState, newState)) return;

            if (_stateTriggers.TryGetValue(CurrientState, out var currentStateEvent))
            {
                currentStateEvent.ExitInvoke();
            }
            CurrientState = newState;

            if (_stateTriggers.TryGetValue(CurrientState, out var newStateEvent))
            {
                newStateEvent.EnteredInvoke();
            }
        }
    }
}

