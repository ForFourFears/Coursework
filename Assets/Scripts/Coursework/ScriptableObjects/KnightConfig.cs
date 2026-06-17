using Coursework.EnumsCreatures.Knight;
using System;
using System.Collections.Generic;
using UnityEngine;
using Coursework.LogicControllers.AttackSystems;
using Unity.VisualScripting;

namespace Coursework.ScriptableObjects
{
    [CreateAssetMenu(fileName = "KnightConfig", menuName = "Configs/KnightConfig")]
    public class KnightConfig : BaseCharacterConfig<KnightActionStates, KnightActions>
    {
        [SerializeReference, SelectSubclassData]
        private List<BaseStateData<KnightActionStates>> _statesList = new();

        [SerializeReference, SelectSubclassData]
        private List<BaseActionData<KnightActions>> _actionsList = new();

        protected override List<BaseStateData<KnightActionStates>> StatesList => _statesList;
        protected override List<BaseActionData<KnightActions>> ActionsList => _actionsList;

        
    }

    #region StatesData
    [Serializable]
    public class KnightState : StateData<KnightActionStates> { }


    [Serializable]
    public class KnightJumpAction : BaseActionData<KnightActions>
    {
        public override KnightActions TargetAction
        {
            get => KnightActions.Jump;
            protected set { }
        }

        [Header("Jump Settings")]
        [field: Min(0)]
        [field: SerializeField] public float JumpModifier { get; private set; } = 10f;

        [field: Min(0)]
        [field: SerializeField] public float CoyoteTime { get; private set; } = 0.15f;

        [field: Min(1)]
        [field: SerializeField] public int NumberOfJumps { get; private set; } = 1;


        public override void OnValidateAction() { }
    }
    #endregion

    #region ActionsData
    [Serializable]
    public class KnightAttackAction : BaseActionData<KnightActions>
    {
        public override KnightActions TargetAction
        {
            get => KnightActions.Attack;
            protected set { }
        }

        [Header("Attack Settings")]
        [field: Min(0)]
        [field: SerializeField] public float CombateTime { get; private set; } = 0.3f;

        [Serializable]
        public struct AttackInfo
        {
            public AttackType AttackType;

            public float Damage;

            public AttackInfo(AttackType attackType, float damage)
            {
                AttackType = attackType;
                Damage = damage;
            }
        }

        [SerializeField] private List<AttackInfo> _attacksInfo = new();

        public List<AttackInfo> AttacksInfo => _attacksInfo;

        public override void OnValidateAction()
        {
            if (_attacksInfo != null)
            {
                HashSet<AttackType> attacks = new();
                for (int i = 0; i < _attacksInfo.Count; i++)
                {
                    if (_attacksInfo[i].AttackType != AttackType.None && !attacks.Add(_attacksInfo[i].AttackType))
                    {
                        _attacksInfo[i] = new();
                    }

                }
            }
        }
    }

    [Serializable]
    public class KnightDashActionData : BaseActionData<KnightActions>
    {
        public override KnightActions TargetAction
        {
            get => KnightActions.Dash;
            protected set { }
        }

        [Header("Dash Settings")]
        [field: SerializeField, Min(0)] public float DashDistance { get; private set; } = 4f;
        [field: SerializeField, Min(0)] public float StaminaCost { get; private set; } = 15f;


        public override void OnValidateAction() { }
    }
    #endregion
}