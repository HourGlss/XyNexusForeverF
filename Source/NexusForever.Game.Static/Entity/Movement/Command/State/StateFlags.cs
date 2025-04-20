namespace NexusForever.Game.Static.Entity.Movement.Command.State
{
    [Flags]
    public enum StateFlags
    {
        None                 = 0x000000,
        Velocity             = 0x000001,
        Move                 = 0x000002,
        Fall                 = 0x000008,
        Jump                 = 0x000040,
        Unknown80            = 0x000080,
        Unknown100           = 0x000100,
        Unknown200           = 0x000200,
        Unknown400           = 0x000400,
        DoubleJump           = 0x000800,
        RollForward          = 0x001000,
        RollBackward         = 0x002000,
        RotationWhileFalling = 0x040000
    }
}
