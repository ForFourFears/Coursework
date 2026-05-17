using System;

namespace Coursework.LogicControllers.ActionTriggerHubs
{
    public class ActionTrigger : IActionTrigger
    {
        public event Action Triggered;
        public void InvokeTrigger() => Triggered?.Invoke();
    }
}