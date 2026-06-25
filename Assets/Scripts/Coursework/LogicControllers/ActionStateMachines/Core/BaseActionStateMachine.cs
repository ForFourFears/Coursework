using Coursework.AnimationControllers.Core;
using System;
using Coursework.LogicControllers.ModifierSystems;
using System.Collections.Generic;
using Coursework.ScriptableObjects;


namespace Coursework.LogicControllers.ActionStateMachines.Core
{
    public abstract class BaseActionStateMachine<TState, TAction> : IActionStateMachine<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        protected readonly Dictionary<TState, StateEvents<TState>> stateEvents;
        protected readonly Dictionary<TAction, ActionEvent> actionEvents;

        protected readonly ModifierSystem modifierSystem;

        protected readonly IObservableSMBsHandler observableSMBsHandler;

        protected readonly IEntityDataHandler<TState, TAction> entityDataHandler;
        protected readonly ActionTimerRegistry<TAction> cooldownRegistry;

        public BaseActionStateMachine(
            ModifierSystem modifierSystem,
            IObservableSMBsHandler observableSMBsHandler, 
            IEntityDataHandler<TState, TAction> entityDataHandler
        )
        {
            this.modifierSystem = modifierSystem;
            this.observableSMBsHandler = observableSMBsHandler;
            this.entityDataHandler = entityDataHandler;
            stateEvents = new();
            actionEvents = new();
            cooldownRegistry = new();
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
            cooldownRegistry.Update(deltaTime);
            currentStateEvents?.UpdateInvoke();
        }

        public abstract void Subscribe();

        public abstract void Unsubscribe();

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

        protected virtual void OnStateChanged(TState currentState)
        {
            var state = entityDataHandler[currentState];
            float mod = 0;

            if (state != null)
            {
                mod = state.SpeedModifier;
            }

            modifierSystem.StateModifier = mod;
        }

        public abstract bool TryExecuteAction(TAction action);

        protected bool TryTriggerAction(TAction action)
        {
            if (cooldownRegistry.IsActive(action)) return false;
            cooldownRegistry[action] = entityDataHandler[action].Cooldown;
            if (actionEvents.TryGetValue(action, out var actionEvent))
            {
                actionEvent.ActionInvoke();
            }
            return true;
        }

        protected bool TryChangeState(TState newState, TAction action)
        {
            if (cooldownRegistry.IsActive(action)) return false;
            ChangeState(newState);
            cooldownRegistry[action] = entityDataHandler[action].Cooldown;
            if (actionEvents.TryGetValue(action, out var actionEvent))
            {
                actionEvent.ActionInvoke();
            }
            return true;            
        }
    }
}

