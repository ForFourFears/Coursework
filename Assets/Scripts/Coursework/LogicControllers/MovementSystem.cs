using UnityEngine;
using Coursework.LogicControllers.ModifierSystems;

namespace Coursework.LogicControllers
{
    public class MovementSystem
    {
        private readonly IMovementContext movementContext;
        private readonly ModifierSystem modifierSystem;
        private readonly Rigidbody2D rigidbody;
        public MovementSystem(IMovementContext movementContext, ModifierSystem modifierSystem)
        {
            this.movementContext = movementContext;
            this.modifierSystem = modifierSystem;
            rigidbody = movementContext.Rigidbody;
        }

        public void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            float velocityX = movementContext.MoveInput.x * modifierSystem.ApplyModifiers();
            float velocityY = rigidbody.linearVelocityY;
            rigidbody.linearVelocity = new Vector2(velocityX, velocityY);
        }
    }
}
