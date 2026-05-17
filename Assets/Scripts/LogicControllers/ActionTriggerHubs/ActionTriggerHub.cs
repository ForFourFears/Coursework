using System;
using System.Collections.Generic;

namespace Coursework.LogicControllers.ActionTriggerHubs
{
    public class ActionTriggerHub<TAction> : IActionTriggerHub<TAction> where TAction : Enum
    {
        private readonly Dictionary<TAction, ActionTrigger> actionTriggers = new();
        
        public IActionTrigger this[TAction trigger]
        {
            get
            {
                if(!actionTriggers.TryGetValue(trigger, out var actionTrigger))
                {
                    actionTrigger = new ActionTrigger();
                    actionTriggers.Add(trigger, actionTrigger);
                }
                return actionTrigger;
            }
        }

        public void InvokeTrigger(TAction trigger)
        {
            actionTriggers[trigger].InvokeTrigger();
        }
    }
}