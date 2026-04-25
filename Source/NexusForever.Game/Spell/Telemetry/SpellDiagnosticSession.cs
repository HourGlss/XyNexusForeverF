namespace NexusForever.Game.Spell.Telemetry
{
    internal class SpellDiagnosticSession
    {
        public string Id { get; init; }
        public string Label { get; init; }
        public DateTimeOffset StartedAt { get; init; }
    }
}
