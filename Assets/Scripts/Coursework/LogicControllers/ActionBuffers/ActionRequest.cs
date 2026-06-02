using Coursework.EnumsCreatures.Knight;

namespace Coursework.LogicControllers.ActionBuffers
{


    public readonly struct ActionRequest
    {
        public readonly KnightActions Action;
        public readonly float LifeTime;
        public readonly bool IsTimedOut;

        public ActionRequest(KnightActions action, float lifeTime) : this(action, lifeTime, lifeTime <= 0) { }

        private ActionRequest(KnightActions action, float lifeTime, bool isTimedOut)
        {
            Action = isTimedOut ? KnightActions.None : action;
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
