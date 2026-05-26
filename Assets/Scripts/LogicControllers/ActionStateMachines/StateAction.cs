using System;
using UnityEngine.InputSystem.LowLevel;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class StateEvents : IStateEvents
    {
        public event Action Entered;
        public event Action Update;
        public event Action Exit;

        public void EnteredInvoke() => Entered?.Invoke();
        public void UpdateInvoke() => Update?.Invoke();
        public void ExitInvoke() => Exit?.Invoke();
    }
}