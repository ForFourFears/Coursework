using System;
using System.Collections.Generic;
using UnityEngine;
using Coursework.AnimationControllers.Core;
using Coursework.EnumsCreatures.Skeleton;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.AttackSystems;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
using Coursework.LogicControllers.CharactersControllers;

namespace Coursework.LogicControllers.ActionStateMachines.Implementations
{
    public class SkeletonActionStateMachine : BaseActionStateMachine<SkeletonStates, SkeletonActions>, IDamageable
    {
        private readonly IMovementContext movementContext;
        private readonly Rigidbody2D rigidbody;
        private readonly IBaseEntityContext entityContext;

        private readonly IMutableHealth healthSystem;

        private static readonly HashSet<SkeletonStates> NonAutoTransitionalStates = new()
        {
            SkeletonStates.Attack,
            SkeletonStates.Death
        };

        public SkeletonActionStateMachine(
            IMovementContext movementContext,
            IBaseEntityContext entityContext,
            ModifierSystem modifierSystem,
            IMutableHealth healthSystem,
            IObservableSMBsHandler observableSMBsHandler,
            IEntityDataHandler<SkeletonStates, SkeletonActions> entityDataHandler)
            : base(modifierSystem, observableSMBsHandler, entityDataHandler)
        {
            this.movementContext = movementContext;
            rigidbody = movementContext.Rigidbody;
            this.entityContext = entityContext;

            this.healthSystem = healthSystem;

            CurrentState = SkeletonStates.None;
        }

        public override void Subscribe()
        {
            observableSMBsHandler["Attack"].ExitState += CheckTransitions;
            healthSystem.HealthChanged += OnHealthChanged;
        }

        public override void Unsubscribe()
        {
            observableSMBsHandler["Attack"].ExitState -= CheckTransitions;
            healthSystem.HealthChanged -= OnHealthChanged;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!NonAutoTransitionalStates.Contains(CurrentState)) CheckTransitions();

            UpdateConstraints();
        }

        public void CheckTransitions()
        {
            if (CurrentState == SkeletonStates.Death) return;

            if (CurrentState != SkeletonStates.Locomotion)
            {
                ChangeState(SkeletonStates.Locomotion);
            }
        }

        private void UpdateConstraints()
        {
            if (entityContext.IsGrounded && movementContext.MoveInput.x == 0 &&
                movementContext.SlopeAngle <= movementContext.MaxSlopeAngle &&
                CurrentState == SkeletonStates.Locomotion
            )
            {
                rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                return;
            }
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public override bool TryExecuteAction(SkeletonActions action)
        {
            return CurrentState switch
            {
                SkeletonStates.Locomotion => action switch
                {
                    SkeletonActions.TurnAround => TryTurnAround(action),
                    SkeletonActions.Attack => TryChangeState(SkeletonStates.Attack, action),
                    SkeletonActions.Hit => TryTakeHitStance(action),
                    SkeletonActions.React => TryTriggerAction(action),
                    _ => false
                },
                SkeletonStates.Attack => action switch
                {
                    SkeletonActions.TurnAround => TryTurnAround(action),
                    SkeletonActions.Hit => TryTakeHitStance(action),
                    _ => false
                },
                _ => false
            };
        }

        private bool TryTurnAround(SkeletonActions action)
        {
            if (entityDataHandler[CurrentState].SpeedModifier > 0)
            {
                return TryTriggerAction(action);
            }
            return false;
        }

        public void TakeDamage(float damage)
        {
            healthSystem.ApplyDamage(damage);
            TryExecuteAction(SkeletonActions.Hit);
        }

        private bool TryTakeHitStance(SkeletonActions action)
        {
            bool result = TryTriggerAction(action);
            if (result) CheckTransitions();
            return result;
        }

        private void OnHealthChanged(float health, float maxHealth, float delta)
        {
            Debug.Log($"Скелет получил урон: {Mathf.Abs(delta)}, ХП: {health}/{maxHealth}");

            if (health <= 0f)
            {
                ChangeState(SkeletonStates.Death);
            }
        }
    }
}