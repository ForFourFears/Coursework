using UnityEngine;
using Coursework.EnumsCreatures.Knight;
using System.Collections.Generic;
using System.Linq;

namespace Coursework.ScriptableObjects
{
    [CreateAssetMenu(fileName = "KnightConfig", menuName = "Scriptable Objects/CharacterConfigs")]
    public class KnightConfig : CharacterConfig, IStateModifiersHandler<KnightActionStates>
    {
        [SerializeField] private KnightStateSpeed[] _knightStatesModifiers;

        public IReadOnlyDictionary<KnightActionStates, float> StatesModifiers => statesModifiers;

        private Dictionary<KnightActionStates, float> statesModifiers;

        private void OnEnable()
        {
            statesModifiers = _knightStatesModifiers.ToDictionary( knightStateSpeed => knightStateSpeed.State, knightStateSpeed => knightStateSpeed.Modifier);
        }
    }

    [System.Serializable]
    public struct KnightStateSpeed
    {
        public KnightActionStates State;
        public float Modifier;

    }
}

