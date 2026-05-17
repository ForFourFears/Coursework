using System;

namespace Assets.Scripts.LogicController.ActionTriggerHub
{
    public class ActionTrigger : IActionTrigger
    {
        public event Action Triggered;
        public void InvokeTrigger() => Triggered?.Invoke();
    }
}