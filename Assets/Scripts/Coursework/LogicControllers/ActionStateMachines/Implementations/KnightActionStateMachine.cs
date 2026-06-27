using Coursework.AnimationControllers.Core;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;
using Coursework.LogicControllers.ActionStateMachines.Core;
using Coursework.LogicControllers.AttackSystems;
using Coursework.LogicControllers.CharactersControllers;

namespace Coursework.LogicControllers.ActionStateMachines.Implementations
{
    public class KnightActionStateMachine : BaseActionStateMachine<KnightStates, KnightActions>, IDamageable
    {
        private readonly IEntityContext entityContext;
        private readonly IMovementContext movementContext;
        private readonly Rigidbody2D rigidbody;
        private readonly float baseGravityScale;

        private readonly IMutableHealth healthSystem;

        private readonly ActionTimerRegistry<ActionWindows> actionWindowsTimer;
        private readonly ActionTimerRegistry<DashTimers> dashTimers;

        private static readonly HashSet<KnightStates> NonAutoTransitionalStates = new()
        {
            KnightStates.Attack,
            KnightStates.Attack2,
            KnightStates.CrouchAttack,
            KnightStates.Dash,
            KnightStates.Death
        };

        private static readonly HashSet<KnightStates> frozenWindowsTimerStates = new()
        {
            KnightStates.Dash
        };

        private readonly KnightJumpAction jumpData;
        private int jumpCounter;
        private readonly KnightAttackAction attackData;
        private readonly KnightDashAction dashData;
        private int dashCounter;

        public KnightActionStateMachine(
            IEntityContext entityContext,
            IMovementContext movementContext,
            ModifierSystem modifierSystem,
            IMutableHealth healthSystem,
            IObservableSMBsHandler observableSMBsHandler,
            IEntityDataHandler<KnightStates, KnightActions> entityDataHandler)
            : base(modifierSystem, observableSMBsHandler, entityDataHandler)
        {
            this.entityContext = entityContext;
            this.movementContext = movementContext;
            rigidbody = movementContext.Rigidbody;
            baseGravityScale = rigidbody.gravityScale;

            this.healthSystem = healthSystem;

            actionWindowsTimer = new();
            dashTimers = new();

            if (entityDataHandler[KnightActions.Jump] is KnightJumpAction jumpConfig)
            {
                jumpData = jumpConfig;
            }
            else throw new System.NullReferenceException("No data for jumpData");

            if (entityDataHandler[KnightActions.Attack] is KnightAttackAction attackConfig)
            {
                attackData = attackConfig;
            }
            else throw new System.NullReferenceException("No data for attackData");

            if (entityDataHandler[KnightActions.Dash] is KnightDashAction dashConfig)
            {
                dashData = dashConfig;
            }
            else throw new System.NullReferenceException("No data for dashData");

            CurrentState = KnightStates.None;

            jumpCounter = jumpData.NumberOfJumps;
            dashCounter = dashData.NumberOfDashCharges;
        }

        public override void Subscribe()
        {
            observableSMBsHandler["Attack"].ExitState += OnAttackStateEnd;
            this[KnightStates.Attack].OnExit += ActivateCombatWindow;

            observableSMBsHandler["Attack2"].ExitState += OnAttackStateEnd;
            observableSMBsHandler["CrouchAttack"].ExitState += OnStateCrouchAttackEnd;

            this[KnightStates.Dash].OnEnter += OnDashStateEntered;
            this[KnightStates.Dash].OnExit += OnDashStateExited;

            healthSystem.HealthChanged += OnHealthChanged;
        }

        public override void Unsubscribe()
        {
            observableSMBsHandler["Attack"].ExitState -= OnAttackStateEnd;
            this[KnightStates.Attack].OnExit -= ActivateCombatWindow;

            observableSMBsHandler["Attack2"].ExitState -= OnAttackStateEnd;
            observableSMBsHandler["CrouchAttack"].ExitState -= OnStateCrouchAttackEnd;

            this[KnightStates.Dash].OnEnter -= OnDashStateEntered;
            this[KnightStates.Dash].OnExit -= OnDashStateExited;

            healthSystem.HealthChanged -= OnHealthChanged;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            dashTimers.Update(deltaTime);
            if (!frozenWindowsTimerStates.Contains(CurrentState)) actionWindowsTimer.Update(deltaTime);

            CheckReturnDashCharge();

            if (entityContext.IsGrounded)
            {
                actionWindowsTimer[ActionWindows.CoyoteJump] = jumpData.CoyoteTime;
                jumpCounter = jumpData.NumberOfJumps;
            }
            else if (!actionWindowsTimer.IsActive(ActionWindows.CoyoteJump) &&
                jumpCounter == jumpData.NumberOfJumps)
            {
                jumpCounter--;
            }

            switch (CurrentState)
            {
                case KnightStates.Dash:
                    if (!dashTimers.IsActive(DashTimers.DashDuration))
                    {
                        CheckTransitions();
                    }
                    break;

                case KnightStates state when NonAutoTransitionalStates.Contains(state):
                    break;

                default:
                    CheckTransitions();
                    break;
            }

            UpdateConstraints();
        }

        private void CheckTransitions()
        {
            if (CurrentState == KnightStates.Death) return;

            if (!entityContext.IsGrounded && Mathf.Abs(rigidbody.linearVelocityY) != 0)
            {
                ChangeState(KnightStates.Air);
            }
            else
            {
                if (entityContext.IsCrouched)
                {
                    ChangeState(KnightStates.Crouch);
                }
                else if (!(entityContext.IsCrouched || entityContext.IsCeilingAbove))
                {
                    ChangeState(KnightStates.Locomotion);
                }
            }
        }

        private void UpdateConstraints()
        {
            if (entityContext.IsGrounded && movementContext.MoveInput.x == 0 && 
                movementContext.SlopeAngle <= movementContext.MaxSlopeAngle &&
                (CurrentState == KnightStates.Locomotion || CurrentState == KnightStates.Crouch)
            )
            {
                rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                return;
            }
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        protected override void OnStateChanged(KnightStates currentState)
        {
            base.OnStateChanged(currentState);
        }

        public override bool TryExecuteAction(KnightActions action)
        {
            return CurrentState switch
            {
                KnightStates.Locomotion => action switch
                {
                    KnightActions.TurnAround => TryTurnAround(action),
                    KnightActions.Jump => TryJump(action),
                    KnightActions.Attack => Attack(action),
                    KnightActions.Dash => TryDash(action),
                    KnightActions.Hit => TryTakeHitStance(action),
                    _ => false
                },
                KnightStates.Air => action switch
                {
                    KnightActions.TurnAround => TryTurnAround(action),
                    KnightActions.Jump => TryJump(action),
                    KnightActions.Attack => Attack(action),
                    KnightActions.Dash => TryDash(action),
                    KnightActions.Hit => TryTakeHitStance(action),
                    _ => false
                },
                KnightStates.Crouch => action switch
                {
                    KnightActions.TurnAround => TryTurnAround(action),
                    KnightActions.Jump => TryJump(action),
                    KnightActions.Attack => TryChangeState(KnightStates.CrouchAttack, action),
                    KnightActions.Dash => TryDash(action),
                    KnightActions.Hit => TryTakeHitStance(action),
                    _ => false
                },
                KnightStates.Attack or KnightStates.Attack2 => action switch 
                {
                    KnightActions.TurnAround => TryTurnAround(action),
                    KnightActions.Jump => TryJump(action),
                    KnightActions.Dash => TryDash(action),
                    KnightActions.Hit => TryTakeHitStance(action),
                    _ => false
                },
                KnightStates.CrouchAttack => action switch
                {
                    KnightActions.TurnAround => TryTurnAround(action),
                    KnightActions.Jump => TryJump(action),
                    KnightActions.Dash => TryDash(action),
                    KnightActions.Hit => TryTakeHitStance(action),
                    _ => false
                },
                KnightStates.Dash => action switch 
                {
                    KnightActions.TurnAround => TryTurnAround(action),
                    KnightActions.Jump => TryJump(action),
                    KnightActions.Attack => Attack(action),
                    KnightActions.Hit => TryTakeHitStance(action),
                    _ => false
                },
                _ => false
            };
        }

        private bool TryTurnAround(KnightActions action)
        {
            if (entityDataHandler[CurrentState].SpeedModifier > 0)
            {
                return TryTriggerAction(action);
            }
            return false;
        }

        private bool TryJump(KnightActions action)
        {
            if (jumpCounter > 0 && !entityContext.IsCeilingAbove)
            {
                bool isCompleted = TryChangeState(KnightStates.Air, action);
                if (isCompleted)
                {
                    jumpCounter--;
                    actionWindowsTimer[ActionWindows.CoyoteJump] = 0;
                }
                return isCompleted;
            }
            return false;
        }

        private bool Attack(KnightActions action)
        {
            if (actionWindowsTimer.IsActive(ActionWindows.Combat))
            {
                bool isCompleted = TryChangeState(KnightStates.Attack2, action);
                if (isCompleted) actionWindowsTimer[ActionWindows.Combat] = 0;
                return isCompleted;
            }
            return TryChangeState(KnightStates.Attack, action);
        }

        private bool TryDash(KnightActions actions)
        {
            if (!entityContext.IsCeilingAbove && dashCounter > 0)
            {
                bool isCompleted = TryChangeState(KnightStates.Dash, actions);

                if (isCompleted)
                {
                    dashCounter--;
                    dashTimers[DashTimers.DashDuration] = dashData.Duration;

                    if (!dashTimers.IsActive(DashTimers.DashChargeCooldown))
                    {
                        dashTimers[DashTimers.DashChargeCooldown] = dashData.DashChargeCooldown;
                    }
                }

                return isCompleted;
            }
            return false;
        }

        private void CheckReturnDashCharge()
        {
            if (dashCounter < dashData.NumberOfDashCharges && !dashTimers.IsActive(DashTimers.DashChargeCooldown))
            {
                dashCounter++;
                if (dashCounter < dashData.NumberOfDashCharges)
                {
                    dashTimers[DashTimers.DashChargeCooldown] = dashData.DashChargeCooldown;
                }
            }
        }

        private void ActivateCombatWindow(KnightStates context)
        {
            actionWindowsTimer[ActionWindows.Combat] = attackData.CombateTime;
        }

        private void OnAttackStateEnd()
        {
            if (CurrentState == KnightStates.Dash) return;
            CheckTransitions();
        }

        private void OnStateCrouchAttackEnd()
        {
            if (CurrentState == KnightStates.Dash) return;
            ChangeState(KnightStates.Crouch);
        }

        private void OnDashStateEntered(KnightStates context)
        {
            rigidbody.gravityScale = 0;
        }

        private void OnDashStateExited(KnightStates context)
        {
            rigidbody.gravityScale = baseGravityScale;
        }

        public void TakeDamage(float damage)
        {
            healthSystem.ApplyDamage(damage);
            TryExecuteAction(KnightActions.Hit);
        }

        private bool TryTakeHitStance(KnightActions action)
        {
            bool result = TryTriggerAction(action);
            if (result) CheckTransitions();
            return result;
        }

        private void OnHealthChanged(float health, float maxHealth, float delta)
        {
            Debug.Log($"Получен урон: {Mathf.Abs(delta)}, осталось хп: {health}/{maxHealth}");
            if (health <= 0) ChangeState(KnightStates.Death); 
        }
    }
}