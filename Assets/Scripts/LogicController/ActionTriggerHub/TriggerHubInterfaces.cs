using System;

namespace Coursework.Scripts.LogicController.ActionTriggerHub
{
    #region StateMachineEnums
    public enum PlayerActionTrigger
    {
        FallStarted,
        TurnAround
    }
    #endregion

    public interface IActionTrigger
    {
        event Action Triggered;
    }

    public interface IActionTriggerHub<TAction> where TAction : Enum
    {
        IActionTrigger this[TAction state] { get; }
    }

    public interface ITriggerHubProvider<TAction> where TAction : Enum
    {
        public IActionTriggerHub<TAction> TriggerHub { get; }
    }
}