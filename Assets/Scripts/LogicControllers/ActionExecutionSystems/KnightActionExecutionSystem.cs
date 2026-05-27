using UnityEngine;
using Coursework.EnumsCreatures.Knight;
using Coursework.LogicControllers.ActionStateMachines;

namespace Coursework.LogicControllers.ActionExecutionSystems
{
    public class KnightActionExecutionSystem : BaseActionExecutionSystem
    {
        private readonly IActionStateMachine<KnightActionStates, KnightActions> actionStateMachine;
        private readonly IMovementContext movementContext;
        public KnightActionExecutionSystem(IMovementContext movementContext, IActionStateMachine<KnightActionStates, KnightActions> actionStateMachine)
        {
            this.actionStateMachine = actionStateMachine;
            this.movementContext = movementContext;
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
            movementContext.Rigidbody.AddForceY(5f, ForceMode2D.Impulse);
        }
    }
}
