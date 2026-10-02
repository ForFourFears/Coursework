namespace Coursework.LogicControllers.ModifierSystems
{
    public interface IModifierSystem
    {
        public bool IgnoreMovementUpdates { get; }
        public float StateModifier { get; }

        public float ApplyModifiers();
    }

    public interface IMutableModifierSystem : IModifierSystem
    {
        new bool IgnoreMovementUpdates { get; set;  }
        new float StateModifier { get; set; }

    }

    public class ModifierSystem : IMutableModifierSystem
    {
        public bool IgnoreMovementUpdates  { get; set; }
        public float StateModifier  { get; set; } = 1;
        //public List<float> EffectsModifiers  { get; set; }

        //EffectsModifiers = new();

        public float ApplyModifiers()
        {
            float result = StateModifier;
            // if (EffectsModifiers.Count != 0)
            // {
            //     foreach (float modifier in EffectsModifiers)
            //     {
            //         result *= modifier;
            //     }
            // }
            return result;
        }

    }
}
