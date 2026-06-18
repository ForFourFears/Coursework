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
    public enum KnightStates
    {
        None = 0,
        Locomotion,
        Air,
        Crouch,
        WallInteraction,
        TurnAround,
        Attack,
        Attack2,
        CrouchAttack,
        Roll,
        Dash,
        Slide,
        Death
    }

    public enum KnightActionWindows
    {
        None = 0,
        CoyoteJump,
        Combat,
        DashDuration
    }

}
