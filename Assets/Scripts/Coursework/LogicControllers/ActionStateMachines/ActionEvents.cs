
using System;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public interface IActionEvent
    {
        event Action Action;
    }
    public class ActionEvent : IActionEvent
    {
        public event Action Action;

        public void ActionInvoke() => Action?.Invoke();
    }
}
