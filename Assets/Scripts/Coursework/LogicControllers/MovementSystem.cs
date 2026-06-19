using Coursework.LogicControllers.ModifierSystems;
using UnityEngine;
using UnityEngine.Splines;

namespace Coursework.LogicControllers
{
    public class MovementSystem
    {
        private readonly IMovementContext movementContext;
        private readonly IEntityContext entityContext;
        private readonly ModifierSystem modifierSystem;
        private readonly Rigidbody2D rigidbody;

        public MovementSystem(IMovementContext movementContext, IEntityContext entityContext,ModifierSystem modifierSystem)
        {
            this.movementContext = movementContext;
            this.entityContext = entityContext;
            this.modifierSystem = modifierSystem;
            rigidbody = movementContext.Rigidbody;
        }

        //public void Subscribe()
        //{

        //}

        //public void Unsubscribe()
        //{

        //}

        public void FixedUpdate()
        {
            if (rigidbody.linearVelocityY < -movementContext.MaxFallSpeed)
            {
                rigidbody.linearVelocityY = -movementContext.MaxFallSpeed;
            }
            Move();
        }

        //public void UpdateVelocityInfo()
        //{
        //    targetSpeed = movementContext.MoveInput.x * modifierSystem.ApplyModifiers();
        //    desiredVelocity = movementContext.SlopeDirection * targetSpeed;
        //}

        private void Move()
        {
            float targetSpeed = movementContext.MoveInput.x * modifierSystem.ApplyModifiers();
            if (modifierSystem.StateModifier == 0) return;
            Vector2 desiredVelocity = movementContext.SlopeDirection * Mathf.Abs(targetSpeed);
            if (entityContext.IsGrounded && rigidbody.linearVelocityY <= desiredVelocity.y)
            {
                rigidbody.linearVelocity = desiredVelocity;
            }
            else
            {
                rigidbody.linearVelocity = new Vector2(targetSpeed, rigidbody.linearVelocityY);
            }
        }
    }
}
