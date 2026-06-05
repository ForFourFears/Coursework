using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionStateMachines;
using Coursework.ScriptableObjects;
using UnityEngine;

namespace Coursework.LogicControllers.ActionExecutionSystems
{
    public class KnightActionExecutionSystem : BaseActionExecutionSystem
    {
        private readonly IActionStateMachine<KnightActionStates, KnightActions> actionStateMachine;
        private readonly IMovementContext movementContext;
        private readonly IActionsModifiersHandler<KnightActions> actionModifiersHandler;
        public KnightActionExecutionSystem(IMovementContext movementContext, IActionStateMachine<KnightActionStates, KnightActions> actionStateMachine, IActionsModifiersHandler<KnightActions> actionModifiersHandler)
        {
            this.movementContext = movementContext;
            this.actionStateMachine = actionStateMachine;
            this.actionModifiersHandler = actionModifiersHandler;
        }

        public override void Subscribe()
        {
            actionStateMachine[KnightActions.Jump].Action += OnJump;
        }
        public override void Unsubscribe()
        {
            actionStateMachine[KnightActions.Jump].Action -= OnJump;
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
    }
}
