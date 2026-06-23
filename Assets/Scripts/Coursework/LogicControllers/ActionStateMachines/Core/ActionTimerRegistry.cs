using System;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines.Core
{
    public class ActionTimerRegistry<TAction> where TAction : Enum
    {
        //Enum.GetValues(typeof(TAction)).Length
        private readonly TAction[] timerNames;
        private readonly float[] timerTimes;

        public ActionTimerRegistry()
        {
            timerNames = (TAction[])Enum.GetValues(typeof(TAction));
            timerTimes = new float[timerNames.Length];
        }

        public float this[TAction action]
        {
            get
            {
                int index = Array.IndexOf(timerNames, action);
                return timerTimes[index];
            }

            set
            {
                int index = Array.IndexOf(timerNames, action);
                timerTimes[index] = Mathf.Max(0, value);
            }
        }

        public bool IsActive(TAction action, float threshold = 0)
        {
            int index = Array.IndexOf(timerNames, action);
            return timerTimes[index] > threshold;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < timerTimes.Length; i++)
            {
                timerTimes[i] = Mathf.Max(0, timerTimes[i] - deltaTime);
            }
        }
    }
}
