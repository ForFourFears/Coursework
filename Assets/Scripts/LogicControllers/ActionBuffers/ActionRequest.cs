using System;
using System.Collections.Generic;
using System.Drawing;

namespace Coursework.LogicControllers.ActionBuffers
{
    public enum PlayerActions
    {
        None,
        Jump,
        Crouch,
        Attack,
        Roll,
        Dash,
        Slide
    }

    public readonly struct ActionRequest
    {
        public readonly PlayerActions Action;
        public readonly float LifeTime;
        public readonly bool IsTimedOut;

        public ActionRequest(PlayerActions action, float lifeTime) : this(action, lifeTime, lifeTime <= 0) { }

        private ActionRequest(PlayerActions action, float lifeTime, bool isTimedOut)
        {
            Action = isTimedOut ? PlayerActions.None : action;
            LifeTime = lifeTime;
            IsTimedOut = isTimedOut;
        }

        public ActionRequest UpdateLifeTime(float deltaTime)
        {
            if (IsTimedOut) return this;
            float lifeTime = LifeTime - deltaTime;
            return new ActionRequest(Action, lifeTime);
        }

        public ActionRequest Overwrite(float defaultLifetime)
        {
            return new ActionRequest(Action, defaultLifetime);
        }
    }
}
