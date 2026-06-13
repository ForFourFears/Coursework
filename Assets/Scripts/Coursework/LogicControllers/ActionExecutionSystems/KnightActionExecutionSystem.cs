using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionStateMachines;
using Coursework.LogicControllers.AttackSystems;
using Coursework.ScriptableObjects;
using Coursework.LogicControllers;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.LogicControllers.ActionExecutionSystems
{
    public interface IAttacker
    {
        public void OnHit(Collider2D target, AttackInfo attack);
    }
    public class KnightActionExecutionSystem : BaseActionExecutionSystem, IAttacker
    {
        private readonly IActionStateMachine<KnightActionStates, KnightActions> actionStateMachine;
        private readonly IMovementContext movementContext;
        private readonly IActionsModifiersHandler<KnightActions> actionModifiersHandler;
        private readonly HashSet<IDamageable> damagedTargets;

        public KnightActionExecutionSystem(
            IMovementContext movementContext, 
            IActionStateMachine<KnightActionStates, KnightActions> actionStateMachine, 
            IActionsModifiersHandler<KnightActions> actionModifiersHandler)
        {
            this.movementContext = movementContext;
            this.actionStateMachine = actionStateMachine;
            this.actionModifiersHandler = actionModifiersHandler;

            damagedTargets = new();
        }

        public override void Subscribe()
        {
            actionStateMachine[KnightActions.Jump].Action += OnJump;
            actionStateMachine[KnightActions.Attack].Action += ResetAttackMemory;
        }
        public override void Unsubscribe()
        {
            actionStateMachine[KnightActions.Jump].Action -= OnJump;
            actionStateMachine[KnightActions.Attack].Action -= ResetAttackMemory;
        }

        public override void Update()
        {
            base.Update();
        }

        private void OnJump()
        {
            if (movementContext.Rigidbody.linearVelocityY < 0)
            {
                movementContext.Rigidbody.linearVelocityY = 0;
            }
            if (!actionModifiersHandler.ActionsModifiers.TryGetValue(KnightActions.Jump, out float mod))
            {
                mod = 2;
            }
            movementContext.Rigidbody.AddForceY(mod, ForceMode2D.Impulse);
        }

        public void OnHit(Collider2D target, AttackInfo attackInfo)
        {
            if (target.TryGetComponent<IDamageable>(out var damageable))
            {
                if (damagedTargets.Add(damageable))
                {
                    if (!actionModifiersHandler.ActionsModifiers.TryGetValue(KnightActions.Attack, out float damage))
                    {
                        damage = 5;
                    }

                    damage = attackInfo.AttackType switch
                    {
                        AttackType.Attack2 => damage * 1.1f,
                        AttackType.CrouchAttack => damage * 0.9f,
                        _ => damage,
                    };
                    damageable.TakeDamage(damage);
                }
            }
        }

        private void ResetAttackMemory()
        {
            damagedTargets.Clear();
        }
    }
}
