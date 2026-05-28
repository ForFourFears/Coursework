using UnityEngine;
using System.Collections.Generic;
using System;

namespace Coursework.ScriptableObjects
{
    public interface IStateModifiersHandler<TState> where TState : Enum
    {
        public IReadOnlyDictionary<TState, float> StatesModifiers { get; }
    }

    public abstract class CharacterConfig : ScriptableObject
    {
        [field: SerializeField] public int Health { get; protected set; }

    }
}

