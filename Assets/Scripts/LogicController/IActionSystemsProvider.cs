using System;
using Assets.Scripts.LogicController.ActionTriggerHub;
using Assets.Scripts.LogicController.ActionStateMachine;
using UnityEngine;
using Assets.Scripts.Animation;

namespace Assets.Scripts.LogicController
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