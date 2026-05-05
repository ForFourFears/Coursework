using System;
using System.Collections.Generic;
using UnityEngine;


namespace Coursework.Scripts.Animation
{
    public class ObservableSMBsHub : MonoBehaviour
    {
        public Dictionary<int, ObservableSMB> AnimationStates { get; private set; }
        public ObservableSMB this [string stateName]
        {
            get
            {
                if (AnimationStates == null) Initialize();
                var stateHash = Animator.StringToHash(stateName);
                return AnimationStates[stateHash];
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (AnimationStates != null) return;
            AnimationStates = new Dictionary<int, ObservableSMB>();
            var animator = GetComponent<Animator>();
            var behaviours = animator.GetBehaviours<ObservableSMB>();
            foreach (var behaviour in behaviours)
            {
                var stateHash = Animator.StringToHash(behaviour.StateName);
                AnimationStates.Add(stateHash, behaviour);
            }
        }
    }
}

