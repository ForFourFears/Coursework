using System;
using UnityEngine;
using Coursework.LogicControllers;
using Coursework.LogicControllers.ActionStateMachines;
using System.Collections.Generic;

namespace Coursework.AnimationControllers
{
    //[RequireComponent(typeof(Animator))]
    public abstract class BaseAnimationController<TState, TAction> 
        where TState : Enum
        where TAction : Enum
    {
        protected static readonly AnimationData entryL1 = new(Animator.StringToHash("Empty"), 1);

        protected Animator animator;

        protected IActionStateMachine<TState, TAction> actionStateMachine;
        protected Rigidbody2D rb;

        protected IObservableSMBsHandler animationEventsHandler;
        protected int[] currentLayerHashes;
        protected readonly HashSet<ObservableSMB> ActiveOSMBs = new();

        public BaseAnimationController(Rigidbody2D rigidbody, Animator animator, IActionStateMachine<TState, TAction> actionStateMachine, IObservableSMBsHandler animationEventsHandler)
        {
            rb = rigidbody;
            this.actionStateMachine = actionStateMachine;
            this.animator = animator;
            currentLayerHashes = new int[animator.layerCount];
            this.animationEventsHandler = animationEventsHandler;
            //Subscribe();
        }
        protected void ChangeAnimation(AnimationData newAnim, float crossFadeDuration = 0)
        {
            int layer = newAnim.Layer;
            int hash = newAnim.Hash;
            if (currentLayerHashes[layer] == hash) return;

            animator.CrossFade(hash, crossFadeDuration, layer);
            currentLayerHashes[layer] = hash;
        }

        protected void PlaySequence(AnimationData currentAnimation, AnimationData? nextAnim = null)
        {
            AnimationData targetAnim = nextAnim ?? entryL1;
            ObservableSMB smb = animationEventsHandler[currentAnimation.Hash];
            if (!ActiveOSMBs.Add(smb))
            {
                return;
            }
            Action onSuccess = null;
            Action onFailure = null;
            void Unsubscribe()
            {
                smb.AnimCycleEnd -= onSuccess;
                smb.ExitState -= onFailure;
                ActiveOSMBs.Remove(smb);
            }

            onSuccess = () =>
            {
                Unsubscribe();
                ChangeAnimation(targetAnim);
            };
            onFailure = () =>
            {
                Unsubscribe();
            };

            smb.AnimCycleEnd += onSuccess;
            smb.ExitState += onFailure;

            ChangeAnimation(currentAnimation);
        }

        public abstract void Subscribe();
        public abstract void Unsubscribe();
    }

    public readonly struct AnimationData
    {
        public int Hash { get; }
        public int Layer { get; }

        public AnimationData(int hash, int layer)
        {
            Hash = hash;
            Layer = layer;
        }
    }
}

