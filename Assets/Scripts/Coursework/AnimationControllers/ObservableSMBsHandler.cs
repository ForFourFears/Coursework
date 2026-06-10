using System;
using System.Collections.Generic;
using UnityEngine;


namespace Coursework.AnimationControllers
{
    public interface IObservableSMBsHandler
    {
        public ObservableSMB this[string stateName] { get; }

        public ObservableSMB this[int stateHash] { get; }
    }
    public class ObservableSMBsHandler : IObservableSMBsHandler
    {
        private Dictionary<int, ObservableSMB> animationStates;
        private readonly Animator animator;

        public ObservableSMBsHandler(Animator animator)
        {
            this.animator = animator;
            if (animationStates == null) Initialize();
        }
        public ObservableSMB this [string stateName]
        {
            get
            {
                if (animationStates == null) Initialize();
                var stateHash = Animator.StringToHash(stateName);
                return animationStates[stateHash];
            }
        }

        public ObservableSMB this [int stateHash]
        {
            get
            {
                if (animationStates == null) Initialize();
                return animationStates[stateHash];
            }
        }

        public void Initialize()
        {
            if (animationStates != null) return;
            animationStates = new Dictionary<int, ObservableSMB>();
            var behaviours = animator.GetBehaviours<ObservableSMB>();
            foreach (var behaviour in behaviours)
            {
                var stateHash = Animator.StringToHash(behaviour.StateName);
                animationStates.Add(stateHash, behaviour);
            }
        }
    }
}

