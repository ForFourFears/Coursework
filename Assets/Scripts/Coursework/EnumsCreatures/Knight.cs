namespace Coursework.EnumsCreatures.Knight
{
    public enum KnightActions
    {
        None = 0,
        Jump,
        Crouch,
        Attack,
        Roll,
        Dash,
        Slide,
        Hit
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
        CrouchAttack,
        Roll,
        Dash,
        Slide,
        Death
    }

}
