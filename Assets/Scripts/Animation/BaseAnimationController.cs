using System;
using UnityEngine;
using Assets.Scripts.LogicController;
using Assets.Scripts.LogicController.ActionStateMachine;
using Assets.Scripts.LogicController.ActionTriggerHub;
using Assets.Scripts.Animation;

namespace Assets.Scripts.Animation
{
    [RequireComponent(typeof(Animator))]
    public abstract class BaseAnimationController<TState, TAction> : MonoBehaviour 
        where TState : Enum
        where TAction : Enum
    {

        [SerializeField] protected Animator _characterAnimator;
        protected IActionSystemProvider<TState, TAction> actionSystemProvider;
        protected IActionStateMachine<TState> stateMachine;
        protected IActionTriggerHub<TAction> triggerHub;
        protected ObservableSMBsHub animationEventHub;
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
                animationEventHub = actionSystemProvider.AnimationEventHub;
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

