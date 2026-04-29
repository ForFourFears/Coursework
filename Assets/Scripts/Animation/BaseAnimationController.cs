using System;
using UnityEngine;
using Coursework.Scripts.LogicController;
using Coursework.Scripts.LogicController.ActionStateMachine;
using Coursework.Scripts.LogicController.ActionTriggerHub;


namespace Coursework.Scripts.Animation
{
    public abstract class BaseAnimationController<TState, TAction> : MonoBehaviour 
        where TState : Enum
        where TAction : Enum
    {

        [SerializeField] protected Animator _characterAnimator;
        protected IActionStateMachine<TState> stateMachine;
        protected IActionTriggerHub<TAction> triggerHub;


        protected void Awake()
        {
            _characterAnimator = _characterAnimator != null ? _characterAnimator : GetComponent<Animator>();
            stateMachine ??= GetComponent<IActionSystemProvider<TState, TAction>>().StateMachine;
            if (stateMachine == null)
            {
                Debug.LogError($"[BaseAnimationController] Не удалось найти StateMachine на {gameObject.name}.");
            }
            triggerHub ??= GetComponent<IActionSystemProvider<TState, TAction>>().TriggerHub;
            if (triggerHub == null)
            {
                Debug.LogError($"[BaseAnimationController] Не удалось найти TriggerHub на {gameObject.name}.");
            }
        }
    }
}

