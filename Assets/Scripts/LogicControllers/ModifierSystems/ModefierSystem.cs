using System;
using System.Collections.Generic;

namespace Coursework.LogicControllers.ModifierSystems
{
    public class ModifierSystem
    {
        private readonly float baseModifier;
        public float StateModifier;
        public List<float> EffectsModifiers;

        public ModifierSystem(float baseModifier)
        {
            this.baseModifier = baseModifier;
            StateModifier = 1;
            EffectsModifiers = new();
        }

        public float ApllyModifiers()
        {
            float result = baseModifier * StateModifier;
            if (EffectsModifiers.Count != 0)
            {
                foreach (float modifier in EffectsModifiers)
                {
                    result += modifier;
                }
            }
            return result;
        }

    }
}
