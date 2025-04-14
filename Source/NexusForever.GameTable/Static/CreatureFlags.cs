namespace NexusForever.GameTable.Static
{
    [Flags]
    public enum CreatureFlags
    {
        // research this further, this flag seems to determine if a creature should have default AI or not
        // this is usually seen for entities that are hostile but do not react to combat
        NoAI = 0x2000
    }
}
