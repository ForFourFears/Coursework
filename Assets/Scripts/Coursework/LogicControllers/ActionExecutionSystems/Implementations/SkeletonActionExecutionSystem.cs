using Coursework.EnumsCreatures.Skeleton;
using Coursework.LogicControllers.ActionExecutionSystems.Core;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.AttackSystems;
using Coursework.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.LogicControllers.ActionExecutionSystems.Implementations
{
    public class SkeletonActionExecutionSystem : BaseActionExecutionSystem<SkeletonStates, SkeletonActions>, IAttacker
    {
        private readonly IMovementContext movementContext;
        private readonly Transform transform;

        private readonly HashSet<IDamageable> damagedTargets = new();
        private readonly Dictionary<AttackType, float> attacksDamage = new();

        private readonly SkeletonAttackAction attackData;

        public SkeletonActionExecutionSystem(
            IMovementContext movementContext,
            ITransformComponent transformHandler,
            IActionStateMachine<SkeletonStates, SkeletonActions> actionStateMachine,
            IActionsDataHandler<SkeletonActions> actionsDataHandler
        ) : base(actionStateMachine, actionsDataHandler)
        {
            this.movementContext = movementContext;
            transform = transformHandler.Transform;

            if (actionDataHandler[SkeletonActions.Attack] is SkeletonAttackAction attackConfig)
            {
                attackData = attackConfig;
            }
            else throw new System.NullReferenceException("No data for attackData");

            attacksDamage.Add(attackData.AttackInfo.AttackType, attackData.AttackInfo.Damage);
        }

        public override void Subscribe()
        {
            actionStateMachine[SkeletonActions.TurnAround].Action += OnTurnAround;

            actionStateMachine[SkeletonStates.Attack].OnEnter += ResetAttackMemory;
            actionStateMachine[SkeletonStates.Attack].OnExit += ResetAttackMemory;
        }

        public override void Unsubscribe()
        {
            actionStateMachine[SkeletonActions.TurnAround].Action -= OnTurnAround;

            actionStateMachine[SkeletonStates.Attack].OnEnter -= ResetAttackMemory;
            actionStateMachine[SkeletonStates.Attack].OnExit -= ResetAttackMemory;
        }

        private void OnTurnAround()
        {
            float moveDirection = Mathf.Sign(movementContext.MoveInput.x);
            Vector3 facingDirection = transform.localScale;

            transform.localScale = new Vector3(Mathf.Abs(facingDirection.x) * moveDirection, facingDirection.y, facingDirection.z);
        }

        public void OnHit(Collider2D target, HitInfo hitInfo)
        {
            if (target.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damagedTargets.Add(damageable))
                {
                    float damage = attacksDamage.GetValueOrDefault(hitInfo.AttackType, 0);
                    damageable.TakeDamage(damage);
                }
            }
        }

        private void ResetAttackMemory(SkeletonStates context)
        {
            damagedTargets.Clear();
        }
    }
}
