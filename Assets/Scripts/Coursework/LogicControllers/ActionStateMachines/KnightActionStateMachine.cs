using Coursework.AnimationControllers;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
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
            observableSMBsHandler["Attack2"].ExitState += CheckTransitions;
            observableSMBsHandler["CrouchAttack"].ExitState += OnStateCrouchAttackEnd;
        }

        public void Unsubscribe()
        {
            observableSMBsHandler["Attack"].ExitState -= CheckTransitions;
            observableSMBsHandler["Attack2"].ExitState -= CheckTransitions;
            observableSMBsHandler["CrouchAttack"].ExitState -= OnStateCrouchAttackEnd;
        }

        public override void Update()
        {
            base.Update();
            if (CurrentState != KnightActionStates.Attack && 
                CurrentState != KnightActionStates.CrouchAttack) CheckTransitions();


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
                    KnightActions.Attack => TryChangeState(KnightActionStates.Attack, action),
                    _ => false
                },
                KnightActionStates.Air => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    KnightActions.Attack => TryChangeState(KnightActionStates.Attack, action),
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
            if (entityContext.IsGrounded && !entityContext.IsCeilingAbove)
            {
                return TryChangeState(KnightActionStates.Air, action);
            }
            else return false;
        }

        //private bool CanAttack(KnightActions action)
        //{
        //    if 
        //}

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
