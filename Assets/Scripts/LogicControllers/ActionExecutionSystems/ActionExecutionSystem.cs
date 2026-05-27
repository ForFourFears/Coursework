

namespace Coursework.LogicControllers.ActionExecutionSystems
{
    public abstract class BaseActionExecutionSystem
    {
        public abstract void Subscribe();
        public abstract void Unsubscribe();

        public virtual void Update() { }
    }
}
