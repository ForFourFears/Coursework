using System;
using UnityEngine;

namespace Coursework.AnimationControllers
{
    public class ObservableSMB : StateMachineBehaviour
    {
        [Tooltip("Точное имя состояния в Animator")]
        [field: SerializeField] public string StateName { get; private set; }
        //public int StateHash { get; private set; }

        public event Action EnterState;
        public event Action ExitState;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            EnterState?.Invoke();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ExitState?.Invoke();
        }
        //private void Awake()
        //{
        //    StateHash = Animator.StringToHash(StateName);
        //}
    }
}

