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
            Vector2 desiredVelocity = movementContext.SlopeDirection * targetSpeed;
            if (entityContext.IsGrounded && rigidbody.linearVelocityY <= desiredVelocity.y)
            {
                rigidbody.linearVelocity = movementContext.SlopeDirection * targetSpeed;
            }
            else
            {
                rigidbody.linearVelocity = new Vector2(targetSpeed, rigidbody.linearVelocityY);
            }
        }
    }
}
