using UnityEngine;
using System.Collections.Generic;
using System;

namespace Coursework.ScriptableObjects
{
    public interface IStatesModifiersHandler<TState> where TState : Enum
    {
        public IReadOnlyDictionary<TState, float> StatesModifiers { get; }
    }

    public interface IActionsMofigiersHandler<TAction> where TAction : Enum
    {
        public IReadOnlyDictionary<TAction, float> ActionsModifiers { get; }
    }

    public abstract class CharacterConfig : ScriptableObject
    {
        [field: SerializeField] public int Health { get; protected set; }

    }
}

