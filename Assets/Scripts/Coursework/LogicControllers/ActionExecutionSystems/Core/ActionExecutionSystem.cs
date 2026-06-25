using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.AttackSystems;
using Coursework.ScriptableObjects;
using System;
using UnityEngine;

namespace Coursework.LogicControllers.ActionExecutionSystems.Core
{
    public interface IAttacker
    {
        public void OnHit(Collider2D target, HitInfo attack);
    }

    public abstract class BaseActionExecutionSystem<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        protected readonly IActionStateMachine<TState, TAction> actionStateMachine;
        protected readonly IActionsDataHandler<TAction> actionDataHandler;

        public BaseActionExecutionSystem(
            IActionStateMachine<TState, TAction> actionStateMachine,
            IActionsDataHandler<TAction> actionDataHandler
        )
        {
            this.actionStateMachine = actionStateMachine;
            this.actionDataHandler = actionDataHandler;
        }

        public abstract void Subscribe();
        public abstract void Unsubscribe();

        public virtual void Update() { }
    }
}
