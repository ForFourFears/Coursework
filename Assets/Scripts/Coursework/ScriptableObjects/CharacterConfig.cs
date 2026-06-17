using UnityEngine;
using System.Collections.Generic;
using System;

namespace Coursework.ScriptableObjects
{
    public interface IStatesDataHandler<TState> where TState : Enum
    {
        public BaseStateData<TState> this[TState state] { get; }
    }

    public interface IActionsDataHandler<TAction> where TAction : Enum
    {
        public BaseActionData<TAction> this[TAction action] { get; }
    }

    public interface IEntityDataHandler<TState, TAction> : IStatesDataHandler<TState>, IActionsDataHandler<TAction>
        where TState : Enum
        where TAction : Enum
    { }

    public abstract class BaseCharacterConfig<TState, TAction> : ScriptableObject, IEntityDataHandler<TState, TAction>
        where TState : Enum
        where TAction : Enum
    {
        [field: SerializeField] 
        [field: Min(0)] 
        public int Health { get; protected set; }

        protected abstract List<BaseStateData<TState>> StatesList { get; }
        protected abstract List<BaseActionData<TAction>> ActionsList { get; }

        protected readonly Dictionary<TState, BaseStateData<TState>> statesData = new();
        protected readonly Dictionary<TAction, BaseActionData<TAction>> actionsData = new();

        public BaseStateData<TState> this[TState state]
        {
            get
            {
                if (statesData.TryGetValue(state, out var stateData))
                {
                    return stateData;
                }
                Debug.LogWarning($"CharacterConfig: {state} not found!");
                return null;
            }
        }

        public BaseActionData<TAction> this[TAction action]
        {
            get
            {
                if (actionsData.TryGetValue(action, out var actionData))
                {
                    return actionData;
                }
                Debug.LogWarning($"CharacterConfig: {action} not found!");
                return null;
            }
        }

        protected virtual void OnEnable()
        {
            
            statesData.Clear();
            if (StatesList != null)
            {
                foreach (var stateData in StatesList)
                {
                    if (stateData != null)
                    {
                        statesData[stateData.TargetState] = stateData;
                    }
                }
            }


            actionsData.Clear();

            if (ActionsList != null)
            {
                foreach (var actionData in ActionsList)
                {
                    if (actionData != null)
                    {
                        actionsData[actionData.TargetAction] = actionData;
                    }
                }
            }
        }

        protected void OnValidate()
        {
            if (StatesList != null)
            {
                HashSet<TState> states = new();
                for (int i = 0; i < StatesList.Count; i++)
                {
                    if (StatesList[i] != null && !states.Add(StatesList[i].TargetState))
                    {
                        Debug.LogWarning($"Duplicate item with TargetState: {StatesList[i].TargetState}!");
                        ActionsList[i] = null;
                    }
                    else StatesList[i]?.OnValidateState();
                }
            }

            if (ActionsList != null)
            {
                HashSet<TAction> states = new();
                for (int i = 0; i < ActionsList.Count; i++)
                {
                    if (ActionsList[i] != null && !states.Add(ActionsList[i].TargetAction))
                    {
                        Debug.LogWarning($"Duplicate item with TargetAction: {ActionsList[i].TargetAction}!");
                        ActionsList[i] = null;
                    }
                    else ActionsList[i]?.OnValidateAction();
                }
            }
        }
    }

    //ДАННЫЕ СОСТОЯНИЙ (STATES)
    #region StatesData
    [Serializable]
    public abstract class BaseStateData<TState> where TState : Enum
    {
        public abstract TState TargetState { get; protected set; }

        [field: SerializeField] public float SpeedModifier { get; private set; } = 1f;


        public abstract void OnValidateState();
    }

    [Serializable]
    public class StateData<TState> : BaseStateData<TState> where TState : Enum
    {
        [SerializeField] private TState _targetState;

        public override TState TargetState
        {
            get => _targetState;
            protected set => _targetState = value;
        }


        public override void OnValidateState() { }
    }
    #endregion

    //ДАННЫЕ ДЕЙСТВИЙ (ACTIONS)
    #region ActionsData
    [Serializable]
    public abstract class BaseActionData<TAction> where TAction : Enum
    {
        public abstract TAction TargetAction { get; protected set; }

        //Сюда можно докинуть общие параметры для ВСЕХ экшенов (например, кулдаун)
        [field: SerializeField] public float Cooldown { get; private set; } = 0f;


        public abstract void OnValidateAction();
    }

    [Serializable]
    public class ActionData<TAction> : BaseActionData<TAction> where TAction : Enum
    {
        [SerializeField] private TAction _targetAction;

        public override TAction TargetAction
        {
            get => _targetAction;
            protected set => _targetAction = value;
        }


        public override void OnValidateAction() { }
    }
    #endregion
}