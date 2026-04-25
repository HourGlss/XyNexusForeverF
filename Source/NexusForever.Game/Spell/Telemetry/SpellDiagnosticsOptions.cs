namespace NexusForever.Game.Spell.Telemetry
{
    public class SpellDiagnosticsOptions
    {
        public bool Enable { get; set; }
        public bool TraceAll { get; set; }
        public bool ChatMarkers { get; set; } = true;
        public bool IncludeEffectResults { get; set; } = true;
        public bool IncludePrerequisiteSuccess { get; set; }
        public int MaxChatMessageLength { get; set; } = 256;
        public List<uint> Spell4Ids { get; set; } = [];
        public List<uint> Spell4BaseIds { get; set; } = [];
        public List<string> PlayerNames { get; set; } = [];
    }
}
