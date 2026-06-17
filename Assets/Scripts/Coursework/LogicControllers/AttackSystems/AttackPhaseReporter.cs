using UnityEngine;
using Coursework.LogicControllers.ActionExecutionSystems;

namespace Coursework.LogicControllers.AttackSystems
{
    public enum AttackType
    {
        None = 0,
        BaseAttack,
        Attack2,
        CrouchAttack
    }

    [System.Serializable]
    public struct HitInfo
    {
        public AttackType AttackType;

        [Min(1)] public int AttackPhase;

        public HitInfo(AttackType attackType, int attackPhase)
        {
            this.AttackType = attackType;
            this.AttackPhase = attackPhase;
        }
    }

    public class AttackPhaseReporter : MonoBehaviour
    {
        [SerializeField] private HitInfo hitInfo;

        private IAttacker attacker;

        private void Awake()
        {
            attacker = GetComponentInParent<IAttacker>();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            attacker.OnHit(other, hitInfo);
        }
    }
}
