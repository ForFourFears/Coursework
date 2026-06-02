using System;
using UnityEngine;
using Coursework.LogicControllers;
using Coursework.LogicControllers.ActionStateMachines;

namespace Coursework.AnimationControllers
{
    [RequireComponent(typeof(Animator))]
    public abstract class BaseAnimationController<TState, TAction> : MonoBehaviour 
        where TState : Enum
        where TAction : Enum
    {

        [SerializeField] protected Animator _characterAnimator;

        //protected IActionStateMachine<TState> stateMachine;
        protected ObservableSMBsHub animationEventHub;
        protected Rigidbody2D rb;
        protected int currentAnimationHash;


        //protected void Awake()
        //{

        //}

        protected void ChangeAnimation(int newAnimationHash, float crossFadeDuration)
        {
            if (currentAnimationHash == newAnimationHash) return;

            _characterAnimator.CrossFade(newAnimationHash, crossFadeDuration);
            currentAnimationHash = newAnimationHash;
        }
    }
}

