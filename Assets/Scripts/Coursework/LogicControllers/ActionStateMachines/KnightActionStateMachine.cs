using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ModifierSystems;
using Coursework.ScriptableObjects;
using UnityEngine;

namespace Coursework.LogicControllers.ActionStateMachines
{
    public class KnightActionStateMachine : BaseActionStateMachine<KnightActionStates, KnightActions>
    {
        private readonly IEntityContext entityContext;
        private readonly Rigidbody2D rigidbody;
        private readonly ModifierSystem modifierSystem;
        private readonly IStatesModifiersHandler<KnightActionStates> statesModifiersHandler;
        public KnightActionStateMachine(IEntityContext entityContext, IMovementContext movementContext, ModifierSystem modifierSystem, IStatesModifiersHandler<KnightActionStates> stateModifiersHandler)
        {
            this.entityContext = entityContext;
            rigidbody = movementContext.Rigidbody;
            this.modifierSystem = modifierSystem;
            this.statesModifiersHandler = stateModifiersHandler;
        }

        public override void Update()
        {
            base.Update();
            CheckTransitions();

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

        public override bool TryExecuteAction(KnightActions action)
        {
            return CurrentState switch
            {
                KnightActionStates.Locomotion => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    _ => false
                },
                KnightActionStates.Air => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    _ => false
                },
                KnightActionStates.Crouch => action switch
                {
                    KnightActions.Jump => CanJump(action),
                    _ => false
                },
                _ => false
            };
        }

        private bool CanJump(KnightActions action)
        {
            if (entityContext.IsGrounded)
            {
                return TryChangeState(KnightActionStates.Air, action);
            }
            else return false;
        }
        private void CheckTransitions()
        {
            if (!entityContext.IsGrounded)
            {
                ChangeState(KnightActionStates.Air);
            }
            else if (rigidbody.linearVelocityY <= 0)
            {
                if (entityContext.IsCrouchIntentHeld /*|| entityContext.IsCeilingAbove*/)
                {
                    ChangeState(KnightActionStates.Crouch);
                }
                else if (!(entityContext.IsCrouchIntentHeld || entityContext.IsCeilingAbove))
                {
                    ChangeState(KnightActionStates.Locomotion);
                }
            }
        }
    }
}
