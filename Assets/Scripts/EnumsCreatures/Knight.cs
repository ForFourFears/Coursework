namespace Coursework.EnumsCreatures.Knight
{
    public enum KnightActions
    {
        None,
        Jump,
        Crouch,
        Attack,
        Roll,
        Dash,
        Slide
    }
    public enum KnightActionStates
    {
        None = 0,
        Locomotion,
        Air,
        Crouch,
        WallInteraction,
        TurnAround,
        Attack,
        Roll,
        Dash,
        Slide,
        Death,
        Hit
    }

}
