using System;

namespace Coursework.LogicControllers.ModifierSystems
{
    public enum ModifierType
    {
        Default,
        State,
        Action
    }

    public struct Modifier
    {
        public float Value;
        public ModifierType type;
        public Modifier(float value = 1, ModifierType type = ModifierType.Default)
        {
            Value = value;
            this.type = type;
        }

    }
}