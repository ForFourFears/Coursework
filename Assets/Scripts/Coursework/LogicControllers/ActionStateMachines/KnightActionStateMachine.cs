using Coursework.AnimationControllers;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class KnightActionStateMachine : BaseActionStateMachine<KnightStates, KnightActions>
    {
        private readonly IEntityContext entityContext;
        private readonly Rigidbody2D rigidbody;

        private readonly ActionTimerRegistry<KnightActionWindows> actionWindowsTimer;

        private static readonly HashSet<KnightStates> NonInterruptibleStates = new()
        {
            KnightStates.Attack,
            KnightStates.Attack2,
            KnightStates.CrouchAttack,
            KnightStates.Dash
        };

        private readonly KnightJumpAction jumpData;
        private int jumpCounter;
        private readonly KnightAttackAction attackData;
        private readonly KnightDashAction dashData;


        public KnightActionStateMachine(
            IEntityContext entityContext,
            IMovementContext movementContext,
            ModifierSystem modifierSystem,
            IEntityDataHandler<KnightStates, KnightActions> entityDataHandler,
            IObservableSMBsHandler observableSMBsHandler)
            : base(modifierSystem, observableSMBsHandler, entityDataHandler)
        {
            this.entityContext = entityContext;
            rigidbody = movementContext.Rigidbody;
            actionWindowsTimer = new();

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
        }

        public void Subscribe()
        {
            observableSMBsHandler["Attack"].ExitState += CheckTransitions;
            this[KnightStates.Attack].Exit += ActivateCombatWindow;

            observableSMBsHandler["Attack2"].ExitState += CheckTransitions;
            observableSMBsHandler["CrouchAttack"].ExitState += OnStateCrouchAttackEnd;
        }

        public void Unsubscribe()
        {
            observableSMBsHandler["Attack"].ExitState -= CheckTransitions;
            this[KnightStates.Attack].Exit -= ActivateCombatWindow;

            observableSMBsHandler["Attack2"].ExitState -= CheckTransitions;
            observableSMBsHandler["CrouchAttack"].ExitState -= OnStateCrouchAttackEnd;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            actionWindowsTimer.Update(deltaTime);

            if (entityContext.IsGrounded)
            {
                actionWindowsTimer[KnightActionWindows.CoyoteJump] = jumpData.CoyoteTime;
                jumpCounter = jumpData.NumberOfJumps;
            }
            else if (!actionWindowsTimer.IsActive(KnightActionWindows.CoyoteJump) &&
                jumpCounter == jumpData.NumberOfJumps)
            {
                jumpCounter--;
            }

            switch (CurrentState)
            {
                case KnightStates.Dash:
                    if (!actionWindowsTimer.IsActive(KnightActionWindows.DashDuration))
                    {
                        CheckTransitions();
                    }
                    break;

                case KnightStates state when NonInterruptibleStates.Contains(state):
                    break;

                default:
                    CheckTransitions();
                    break;
            }


        }

        public override bool TryExecuteAction(KnightActions action)
        {
            return CurrentState switch
            {
                KnightStates.Locomotion => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Attack => Attack(action),
                    KnightActions.Dash => CanDash(action),
                    _ => false
                },
                KnightStates.Air => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Attack => Attack(action),
                    KnightActions.Dash => CanDash(action),
                    _ => false
                },
                KnightStates.Crouch => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Attack => TryChangeState(KnightStates.CrouchAttack, action),
                    KnightActions.Dash => CanDash(action),
                    _ => false
                },
                KnightStates.Attack => action switch 
                { 
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Dash => CanDash(action),
                    _ => false
                },
                KnightStates.CrouchAttack => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Dash => CanDash(action),
                    _ => false
                },
                KnightStates.Dash => action switch 
                { 
                    KnightActions.Jump => CanJump(action),
                    _ => false
                },
                _ => false
            };
        }

        private bool CanJump(KnightActions action)
        {
            if (jumpCounter > 0 && !entityContext.IsCeilingAbove)
            {
                bool isCompleted = TryChangeState(KnightStates.Air, action);
                if (isCompleted)
                {
                    jumpCounter--;
                    actionWindowsTimer[KnightActionWindows.CoyoteJump] = 0;
                }
                    
                    
                return isCompleted;
            }
            else return false;
        }

        private bool Attack(KnightActions action)
        {
            if (actionWindowsTimer.IsActive(KnightActionWindows.Combat))
            {
                bool isCompleted = TryChangeState(KnightStates.Attack2, action);
                if (isCompleted) actionWindowsTimer[KnightActionWindows.Combat] = 0;
                return isCompleted;
            }
            return TryChangeState(KnightStates.Attack, action);
        }

        private bool CanDash(KnightActions actions)
        {
            if (!entityContext.IsCeilingAbove)
            {
                bool isCompleted = TryChangeState(KnightStates.Dash, actions);
                if (isCompleted) actionWindowsTimer[KnightActionWindows.DashDuration] = dashData.Duration;
                return isCompleted;
            }
            return false;
        }

        private void ActivateCombatWindow(KnightStates context)
        {
            actionWindowsTimer[KnightActionWindows.Combat] = attackData.CombateTime;
        }

        private void CheckTransitions()
        {
            if (!entityContext.IsGrounded)
            {
                ChangeState(KnightStates.Air);
            }
            else /*if (rigidbody.linearVelocityY <= 0)*/
            {
                if (entityContext.IsCrouched /*|| entityContext.IsCeilingAbove*/)
                {
                    ChangeState(KnightStates.Crouch);
                }
                else if (!(entityContext.IsCrouched || entityContext.IsCeilingAbove))
                {
                    ChangeState(KnightStates.Locomotion);
                }
            }
        }

        private void OnStateCrouchAttackEnd()
        {
            ChangeState(KnightStates.Crouch);
        }
    }
}
