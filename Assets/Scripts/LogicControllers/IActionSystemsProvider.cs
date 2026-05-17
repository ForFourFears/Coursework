using System;
using UnityEngine;
using Coursework.AnimationControllers;
using Coursework.LogicControllers.ActionTriggerHubs;
using Coursework.LogicControllers.ActionStateMachines;

namespace Coursework.LogicControllers
{
    public interface IActionSystemProvider<TState, TAction> :
        IStateMachineProvider<TState>,
        ITriggerHubProvider<TAction>
        where TState : Enum
        where TAction : Enum
    { 
        public Rigidbody2D Rigidbody { get; }
        public ObservableSMBsHub AnimationEventHub { get; }
    }
}