using Coursework.AnimationControllers;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class KnightActionStateMachine : BaseActionStateMachine<KnightActionStates, KnightActions>
    {
        private readonly IEntityContext entityContext;
        private readonly Rigidbody2D rigidbody;
        private readonly ModifierSystem modifierSystem;
        private readonly IEntityDataHandler<KnightActionStates, KnightActions> entityDataHandler;

        private readonly ActionTimerRegistry<KnightActionWindows> actionWindowsTimer;

        private static readonly HashSet<KnightActionStates> NonInterruptibleStates = new()
        {
            KnightActionStates.Attack,
            KnightActionStates.Attack2,
            KnightActionStates.CrouchAttack
        };

        private readonly KnightJumpAction jumpData;
        private int jumpCounter;
        private readonly KnightAttackAction attackData;


        public KnightActionStateMachine(
            IEntityContext entityContext,
            IMovementContext movementContext,
            ModifierSystem modifierSystem,
            IEntityDataHandler<KnightActionStates, KnightActions> entityDataHandler,
            IObservableSMBsHandler observableSMBsHandler)
            : base(observableSMBsHandler)
        {
            this.entityContext = entityContext;
            rigidbody = movementContext.Rigidbody;
            this.modifierSystem = modifierSystem;
            this.entityDataHandler = entityDataHandler;
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
        }

        public void Subscribe()
        {
            observableSMBsHandler["Attack"].ExitState += CheckTransitions;
            this[KnightActionStates.Attack].Exit += ActivateCombatWindow;

            observableSMBsHandler["Attack2"].ExitState += CheckTransitions;
            observableSMBsHandler["CrouchAttack"].ExitState += OnStateCrouchAttackEnd;
        }

        public void Unsubscribe()
        {
            observableSMBsHandler["Attack"].ExitState -= CheckTransitions;
            this[KnightActionStates.Attack].Exit -= ActivateCombatWindow;

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

            if (!NonInterruptibleStates.Contains(CurrentState)) CheckTransitions();



        }

        protected override void OnStateChanged(KnightActionStates currentState)
        {
            var state = entityDataHandler[currentState];
            float mod = 0;

            if (state != null)
            {
                mod = state.SpeedModifier;
            }

            modifierSystem.StateModifier = mod;
        }

        public override bool TryExecuteAction(KnightActions action)
        {
            return CurrentState switch
            {
                KnightActionStates.Locomotion => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Attack => Attack(action),
                    _ => false
                },
                KnightActionStates.Air => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Attack => Attack(action),
                    _ => false
                },
                KnightActionStates.Crouch => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Attack => TryChangeState(KnightActionStates.CrouchAttack, action),
                    _ => false
                },
                KnightActionStates.Attack => action switch 
                { 
                    KnightActions.Jump => CanJump(action),
                    _ => false
                },
                KnightActionStates.CrouchAttack => action switch
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
                bool isCompleted = TryChangeState(KnightActionStates.Air, action);
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
                bool isCompleted = TryChangeState(KnightActionStates.Attack2, action);
                if (isCompleted) actionWindowsTimer[KnightActionWindows.Combat] = 0;
                return isCompleted;
            }
            return TryChangeState(KnightActionStates.Attack, action);
        }

        private void ActivateCombatWindow(KnightActionStates context)
        {
            actionWindowsTimer[KnightActionWindows.Combat] = attackData.CombateTime;
        }

        private void CheckTransitions()
        {
            if (!entityContext.IsGrounded)
            {
                ChangeState(KnightActionStates.Air);
            }
            else /*if (rigidbody.linearVelocityY <= 0)*/
            {
                if (entityContext.IsCrouched /*|| entityContext.IsCeilingAbove*/)
                {
                    ChangeState(KnightActionStates.Crouch);
                }
                else if (!(entityContext.IsCrouched || entityContext.IsCeilingAbove))
                {
                    ChangeState(KnightActionStates.Locomotion);
                }
            }
        }

        private void OnStateCrouchAttackEnd()
        {
            ChangeState(KnightActionStates.Crouch);
        }
    }
}
