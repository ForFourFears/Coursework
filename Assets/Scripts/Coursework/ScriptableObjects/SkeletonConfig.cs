using Coursework.EnumsCreatures.Skeleton;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SkeletonConfig", menuName = "Configs/SkeletonConfig")]
    public class SkeletonConfig : BaseCharacterConfig<SkeletonStates, SkeletonActions>
    {
        [SerializeReference, SelectSubclassData]
        private List<BaseStateData<SkeletonStates>> _statesList = new();

        [SerializeReference, SelectSubclassData]
        private List<BaseActionData<SkeletonActions>> _actionsList = new();

        protected override List<BaseStateData<SkeletonStates>> StatesList => _statesList;
        protected override List<BaseActionData<SkeletonActions>> ActionsList => _actionsList;


    }

    #region StatesData
    [Serializable]
    public class SkeletonState : StateData<SkeletonStates> { }
    #endregion

    #region ActionsData
    [Serializable]
    public class SkeletonAction : ActionData<SkeletonActions> { }

    [Serializable]
    public class SkeletonAttackAction : BaseActionData<SkeletonActions>
    {
        public override SkeletonActions TargetAction
        {
            get => SkeletonActions.Attack;
            protected set { }
        }

        [SerializeField] private AttackInfo attacksInfo;
        public AttackInfo AttackInfo => attacksInfo;

        public override void OnValidateAction()
        {

        }
    }
    #endregion
}
