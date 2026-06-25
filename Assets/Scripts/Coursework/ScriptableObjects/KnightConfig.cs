using Coursework.EnumsCreatures.Knight;
using System;
using System.Collections.Generic;
using UnityEngine;
using Coursework.LogicControllers.AttackSystems;
using Unity.VisualScripting;

namespace Coursework.ScriptableObjects
{
    [CreateAssetMenu(fileName = "KnightConfig", menuName = "Configs/KnightConfig")]
    public class KnightConfig : BaseCharacterConfig<KnightStates, KnightActions>
    {
        [SerializeReference, SelectSubclassData]
        private List<BaseStateData<KnightStates>> _statesList = new();

        [SerializeReference, SelectSubclassData]
        private List<BaseActionData<KnightActions>> _actionsList = new();

        protected override List<BaseStateData<KnightStates>> StatesList => _statesList;
        protected override List<BaseActionData<KnightActions>> ActionsList => _actionsList;

        
    }

    #region StatesData
    [Serializable]
    public class KnightState : StateData<KnightStates> { }
    #endregion

    #region ActionsData
    [Serializable]
    public class KnightAction : ActionData<KnightActions> { }

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
    public class KnightDashAction : BaseActionData<KnightActions>
    {
        public override KnightActions TargetAction
        {
            get => KnightActions.Dash;
            protected set { }
        }

        [Header("Dash Settings")]
        [field: SerializeField, Min(0)] public float SpeedModifier { get; private set; } = 15f;
        [field: SerializeField, Min(0)] public float Duration { get; private set; } = 0.1f;

        [SerializeField] private float _distance;



        public override void OnValidateAction() 
        {
            _distance = SpeedModifier * Duration;
        }
    }
    #endregion
}