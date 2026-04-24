using System;
using UnityEngine;
using Coursework.Scripts.LogicController;


namespace Coursework.Scripts.Animation
{
    public abstract class BaseAnimationController<TState> : MonoBehaviour where TState : Enum
    {

        [SerializeField] protected Animator _characterAnimator;
        protected ActionStateMachine<TState> stateMachine;


        protected void Awake()
        {
            _characterAnimator = _characterAnimator != null ? _characterAnimator : GetComponent<Animator>();
            stateMachine ??= GetComponent<IStateMachineProvider<TState>>().StateMachine;
            if (stateMachine == null)
            {
                Debug.LogError($"[BaseAnimationController] Не удалось найти StateMachine на {gameObject.name}.");
            }
        }
    }
}

