using Coursework.AnimationControllers;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class KnightActionStateMachine : BaseActionStateMachine<KnightActionStates, KnightActions>
    {
        private readonly IEntityContext entityContext;
        private readonly Rigidbody2D rigidbody;
        private readonly ModifierSystem modifierSystem;
        private readonly IStatesModifiersHandler<KnightActionStates> statesModifiersHandler;

        private static readonly HashSet<KnightActionStates> NonInterruptibleStates = new()
        {
            KnightActionStates.Attack,
            KnightActionStates.Attack2,
            KnightActionStates.CrouchAttack
        };

        private readonly float combatTime = 0.3f;
        private float combatTimeCounter;

        public KnightActionStateMachine(
            IEntityContext entityContext,
            IMovementContext movementContext,
            ModifierSystem modifierSystem,
            IStatesModifiersHandler<KnightActionStates> statesModifiersHandler,
            IObservableSMBsHandler observableSMBsHandler)
            : base(observableSMBsHandler)
        {
            this.entityContext = entityContext;
            rigidbody = movementContext.Rigidbody;
            this.modifierSystem = modifierSystem;
            this.statesModifiersHandler = statesModifiersHandler;
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

            if (!NonInterruptibleStates.Contains(CurrentState)) CheckTransitions();

            combatTimeCounter = Mathf.Clamp(combatTimeCounter - deltaTime, 0, combatTime);

        }

        protected override void OnStateChanged(KnightActionStates currentState)
        {
            if (statesModifiersHandler.StatesModifiers.TryGetValue(currentState, out float mod))
            {
                modifierSystem.StateModifier = mod;
            }
            else
            {
                modifierSystem.StateModifier = 0;
            }
        }

        //Проверяю, возможно ли в текущем состоянии это действие.
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
            if ((entityContext.IsGrounded || entityContext.CanCoyoteJump) && !entityContext.IsCeilingAbove)
            {
                return TryChangeState(KnightActionStates.Air, action);
            }
            else return false;
        }

        private bool Attack(KnightActions action)
        {
            if (combatTimeCounter > 0)
            {
                bool isCompleted = TryChangeState(KnightActionStates.Attack2, action);
                if (isCompleted) combatTimeCounter = 0;
                return isCompleted;
            }
            return TryChangeState(KnightActionStates.Attack, action);
        }

        private void ActivateCombatWindow(KnightActionStates context)
        {
            combatTimeCounter = combatTime;
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
