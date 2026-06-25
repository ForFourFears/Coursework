using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers;
using Coursework.LogicControllers.ActionExecutionSystems.Core;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.AttackSystems;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Coursework.LogicControllers.ActionExecutionSystems.Implementations
{

    public class KnightActionExecutionSystem : BaseActionExecutionSystem<KnightStates, KnightActions>, IAttacker
    {
        private readonly IMovementContext movementContext;
        private readonly IEntityContext entityContext;
        private readonly Transform transform;
        private readonly HashSet<IDamageable> damagedTargets;

        private readonly KnightJumpAction jumpData;
        private readonly KnightAttackAction attackData;
        private readonly KnightDashAction dashData;

        private readonly Dictionary<AttackType, float> attacksDamage;

        public KnightActionExecutionSystem(
            IMovementContext movementContext,
            IEntityContext entityContext,
            ITransformComponent transformHandler,
            IActionStateMachine<KnightStates, KnightActions> actionStateMachine,
            IActionsDataHandler<KnightActions> actionDataHandler
        ) : base (actionStateMachine, actionDataHandler)
        {
            this.movementContext = movementContext;
            this.entityContext = entityContext;
            transform = transformHandler.Transform;

            if (actionDataHandler[KnightActions.Jump] is KnightJumpAction jumpConfig)
            {
                jumpData = jumpConfig;
            }
            else throw new System.NullReferenceException("No data for jumpData");

            if (actionDataHandler[KnightActions.Attack] is KnightAttackAction attackConfig)
            {
                attackData = attackConfig;
            }
            else throw new System.NullReferenceException("No data for attackData");

            if (actionDataHandler[KnightActions.Dash] is KnightDashAction dashConfig)
            {
                dashData = dashConfig;
            }
            else throw new System.NullReferenceException("No data for dashData");

            attacksDamage = new();

            for (int i = 0; i < attackData.AttacksInfo.Count; i++)
            {
                if (attackData.AttacksInfo[i].AttackType != AttackType.None)
                {
                    attacksDamage.Add(attackData.AttacksInfo[i].AttackType, attackData.AttacksInfo[i].Damage);
                }
            }

            damagedTargets = new();
        }

        public override void Subscribe()
        {
            actionStateMachine[KnightActions.TurnAround].Action += OnTurnAround;

            actionStateMachine[KnightActions.Jump].Action += OnJump;

            actionStateMachine[KnightStates.Attack].OnEnter += ResetAttackMemory;
            actionStateMachine[KnightStates.Attack].OnExit += ResetAttackMemory;

            actionStateMachine[KnightStates.Attack2].OnEnter += ResetAttackMemory;
            actionStateMachine[KnightStates.Attack2].OnExit += ResetAttackMemory;

            actionStateMachine[KnightStates.CrouchAttack].OnEnter += ResetAttackMemory;
            actionStateMachine[KnightStates.CrouchAttack].OnExit += ResetAttackMemory;

            actionStateMachine[KnightStates.Dash].OnEnter += OnDash;
            actionStateMachine[KnightStates.Dash].OnUpdate += OnDashStateUpdate;
            actionStateMachine[KnightStates.Dash].OnExit += OnDash;
        }
        public override void Unsubscribe()
        {
            actionStateMachine[KnightActions.TurnAround].Action -= OnTurnAround;

            actionStateMachine[KnightActions.Jump].Action -= OnJump;

            actionStateMachine[KnightStates.Attack].OnEnter -= ResetAttackMemory;
            actionStateMachine[KnightStates.Attack].OnExit -= ResetAttackMemory;

            actionStateMachine[KnightStates.Attack2].OnEnter -= ResetAttackMemory;
            actionStateMachine[KnightStates.Attack2].OnExit -= ResetAttackMemory;

            actionStateMachine[KnightStates.CrouchAttack].OnEnter -= ResetAttackMemory;
            actionStateMachine[KnightStates.CrouchAttack].OnExit -= ResetAttackMemory;

            actionStateMachine[KnightStates.Dash].OnEnter -= OnDash;
            actionStateMachine[KnightStates.Dash].OnUpdate -= OnDashStateUpdate;
            actionStateMachine[KnightStates.Dash].OnExit -= OnDash;
        }

        public override void Update()
        {
            base.Update();
        }

        private void OnTurnAround()
        {
            float moveDirection = Mathf.Sign(movementContext.MoveInput.x);
            Vector3 facingDirection = transform.localScale;

            transform.localScale = new Vector3(Mathf.Abs(facingDirection.x) * moveDirection, facingDirection.y, facingDirection.z);
        }

        private void OnJump()
        {
            movementContext.Rigidbody.linearVelocityY = 0;
            float mod = jumpData.JumpModifier;
            movementContext.Rigidbody.AddForceY(mod, ForceMode2D.Impulse);
        }

        private void OnDash(KnightStates contex)
        {
            movementContext.Rigidbody.linearVelocity = Vector2.zero;
        }

        private void OnDashStateUpdate()
        {
            float targetSpeed = dashData.SpeedModifier * entityContext.FacingSign;
            Vector2 desiredVelocity = movementContext.SlopeDirection * targetSpeed;
            if (entityContext.IsGrounded)
            {
                movementContext.Rigidbody.linearVelocity = desiredVelocity;
            }
            else
            {
                movementContext.Rigidbody.linearVelocity = new Vector2(targetSpeed, 0);
            }
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

        private void ResetAttackMemory(KnightStates context)
        {
            damagedTargets.Clear();
        }
    }
}
