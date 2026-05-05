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
        protected IActionSystemProvider<TState, TAction> actionSystemProvider;
        protected IActionStateMachine<TState> stateMachine;
        protected IActionTriggerHub<TAction> triggerHub;
        protected Rigidbody2D rb;
        protected int currentAnimationHash;


        protected void Awake()
        {
            _characterAnimator = _characterAnimator != null ? _characterAnimator : GetComponent<Animator>();
            
            if (!TryGetComponent<IActionSystemProvider<TState, TAction>>(out var actionSystemProvider))
            {
                Debug.LogError($"[BaseAnimationController] Не удалось найти IActionSystemProvider на {gameObject.name}.");
            }
            else
            {
                stateMachine = actionSystemProvider.StateMachine;
                triggerHub = actionSystemProvider.TriggerHub;
                rb = actionSystemProvider.Rigidbody;
            }
        }

        protected void ChangeAnimation(int newAnimationHash, float crossFadeDuration)
        {
            if (currentAnimationHash == newAnimationHash) return;

            _characterAnimator.CrossFade(newAnimationHash, crossFadeDuration);
            currentAnimationHash = newAnimationHash;
        }
    }
}

