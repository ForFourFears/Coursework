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
        public event Action UpdateState;
        public event Action ExitState;
        public event Action AnimCycleEnd;

        private int countCycles;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            EnterState?.Invoke();
            countCycles = 1;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UpdateState?.Invoke();
            if (stateInfo.normalizedTime >= countCycles)
            {
                AnimCycleEnd?.Invoke();
                countCycles++;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ExitState?.Invoke();
            countCycles = 1;
        }

        //private void Awake()
        //{
        //    StateHash = Animator.StringToHash(StateName);
        //}
    }
}

