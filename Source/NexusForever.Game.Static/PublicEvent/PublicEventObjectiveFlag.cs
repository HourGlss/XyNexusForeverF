namespace NexusForever.Game.Static.PublicEvent
{
    [Flags]
    public enum PublicEventObjectiveFlag
    {
        None                    = 0x000000,
        InitialObjective        = 0x000001,
        DynamicObjective        = 0x000080,
        ShouldShowOnMinimapEdge = 0x010000
    }
}
