using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionExecutionSystems.Core;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.AttackSystems;
using Coursework.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;
using Coursework.LogicControllers.CharactersControllers;

namespace Coursework.LogicControllers.ActionExecutionSystems.Implementations
{

    public class KnightActionExecutionSystem : BaseActionExecutionSystem<KnightStates, KnightActions>, IAttacker
    {
        private readonly IMovementContext movementContext;
        private readonly ICrouchInfo _crouchInfo;
        private readonly Transform transform;
        private readonly HashSet<IDamageable> damagedTargets;

        private readonly KnightJumpActionData jumpData;
        private readonly KnightAttackActionData attackData;
        private readonly KnightDashActionData dashData;

        private readonly Dictionary<AttackType, float> attacksDamage;

        public KnightActionExecutionSystem(
            IMovementContext movementContext,
            ICrouchInfo crouchInfo,
            ITransformComponent transformHandler,
            IActionStateMachine<KnightStates, KnightActions> actionStateMachine,
            IActionsDataHandler<KnightActions> actionDataHandler
        ) : base (actionStateMachine, actionDataHandler)
        {
            this.movementContext = movementContext;
            this._crouchInfo = crouchInfo;
            transform = transformHandler.Transform;

            if (actionDataHandler[KnightActions.Jump] is KnightJumpActionData jumpConfig)
            {
                jumpData = jumpConfig;
            }
            else throw new System.NullReferenceException("No data for jumpData");

            if (actionDataHandler[KnightActions.Attack] is KnightAttackActionData attackConfig)
            {
                attackData = attackConfig;
            }
            else throw new System.NullReferenceException("No data for attackData");

            if (actionDataHandler[KnightActions.Dash] is KnightDashActionData dashConfig)
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

        private void OnDash(KnightStates context)
        {
            movementContext.Rigidbody.linearVelocity = Vector2.zero;
        }

        private void OnDashStateUpdate()
        {
            float targetSpeed = dashData.SpeedModifier * _crouchInfo.FacingSign;
            Vector2 desiredVelocity = movementContext.SlopeDirection * targetSpeed;
            if (_crouchInfo.IsGrounded)
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

            var damageable = target.GetComponentInParent<IDamageable>();

            if (damageable != null)
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
