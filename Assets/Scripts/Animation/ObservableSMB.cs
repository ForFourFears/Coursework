using System;
using UnityEngine;

namespace Coursework.Scripts.Animation
{
    public class ObservableSMB : StateMachineBehaviour
    {
        [Tooltip("Точное имя состояния в Animator")]
        [field: SerializeField] public string StateName { get; private set; }
        public int StateHash { get; private set; }

        public event Action EnterState;
        public event Action ExitState;

        private void Awake()
        {
            StateHash = Animator.StringToHash(StateName);
        }
    }
}

