using UnityEngine;
using Coursework.LogicControllers.ActionExecutionSystems;

namespace Coursework.LogicControllers.AttackSystems
{
    public enum AttackType
    {
        BaseAttack,
        Attack2,
        CrouchAttack
    }

    [System.Serializable]
    public struct AttackInfo
    {
        public AttackType AttackType;

        public int AttackPhase;

        public AttackInfo(AttackType attackType, int attackPhase)
        {
            this.AttackType = attackType;
            this.AttackPhase = attackPhase;
        }
    }

    public class AttackPhaseReporter : MonoBehaviour
    {
        [SerializeField] private AttackInfo attackType;

        private IAttacker attacker;

        private void Awake()
        {
            attacker = GetComponentInParent<IAttacker>();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            attacker.OnHit(other, attackType);
        }
    }
}
