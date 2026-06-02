using UnityEngine;
using Coursework.EnumsCreatures.Knight;
using System.Collections.Generic;
using System.Linq;

namespace Coursework.ScriptableObjects
{
    [CreateAssetMenu(fileName = "KnightConfig", menuName = "Scriptable Objects/CharacterConfigs/KnightConfig")]
    public class KnightConfig : CharacterConfig, IStatesModifiersHandler<KnightActionStates>, IActionsMofigiersHandler<KnightActions>
    {
        [SerializeField] private KnightStateModifier[] _knightStatesModifiers;

        [SerializeField] private KnightActionModifier[] _knightActionsModifiers;

        public IReadOnlyDictionary<KnightActionStates, float> StatesModifiers => statesModifiers;

        public IReadOnlyDictionary<KnightActions, float> ActionsModifiers => actionsModifiers;

        private Dictionary<KnightActionStates, float> statesModifiers;

        private Dictionary<KnightActions, float> actionsModifiers;

        private void OnEnable()
        {
            statesModifiers = _knightStatesModifiers.ToDictionary( stateModifier => stateModifier.State, stateModifier => stateModifier.Modifier);
            actionsModifiers = _knightActionsModifiers.ToDictionary(actionModifier => actionModifier.Action, actionModifiers => actionModifiers.Modifier);
        }
    }

    [System.Serializable]
    public struct KnightStateModifier
    {
        public KnightActionStates State;
        public float Modifier;

    }

    [System.Serializable]
    public struct KnightActionModifier
    {
        public KnightActions Action;
        public float Modifier;

    }
}

