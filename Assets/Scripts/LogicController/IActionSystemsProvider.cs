using System;
using Coursework.Scripts.LogicController.ActionTriggerHub;
using Coursework.Scripts.LogicController.ActionStateMachine;
using UnityEngine;
using Coursework.Scripts.Animation;

namespace Coursework.Scripts.LogicController
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