using Coursework.LogicControllers.ModifierSystems;
using UnityEngine;

namespace Coursework.LogicControllers.MovementSystems
{
    public class MovementSystem
    {
        private readonly IMovementContext movementContext;
        private readonly IBaseEntityContext entityContext;
        private readonly ModifierSystem modifierSystem;
        private readonly Rigidbody2D rigidbody;

        public MovementSystem(IMovementContext movementContext, IBaseEntityContext entityContext,ModifierSystem modifierSystem)
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
            float input = movementContext.MoveInput.x;
            float targetSpeed = input * modifierSystem.ApplyModifiers();
            if (modifierSystem.StateModifier == 0) return;
            Vector2 desiredVelocity = movementContext.SlopeDirection * targetSpeed;
            if (entityContext.IsGrounded && rigidbody.linearVelocityY <= desiredVelocity.y && Mathf.Abs(input) > 0.3f && movementContext.SlopeAngle != 0)
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
