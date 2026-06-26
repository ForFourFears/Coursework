using System;
using System.Collections.Generic;

namespace Coursework.LogicControllers.ModifierSystems
{
    public class ModifierSystem
    {
        public bool IgnoreMovementUpdates;
        public float StateModifier;
        public List<float> EffectsModifiers;

        public ModifierSystem()
        {
            StateModifier = 1;
            EffectsModifiers = new();
        }

        public float ApplyModifiers()
        {
            float result = StateModifier;
            if (EffectsModifiers.Count != 0)
            {
                foreach (float modifier in EffectsModifiers)
                {
                    result *= modifier;
                }
            }
            return result;
        }

    }
}
