namespace Coursework.EnumsCreatures.Knight
{
    public enum KnightActions
    {
        None = 0,
        TurnAround,
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
        Attack,
        Attack2,
        CrouchAttack,
        //WallInteraction,
        Roll,
        Dash,
        Slide,
        Death
    }

    public enum ActionWindows
    {
        None = 0,
        CoyoteJump,
        Combat,

    }

    public enum DashTimers
    {
        None = 0,
        DashDuration,
        DashChargeCooldown
    }

}
