using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace NexusForever.Game.Spell.Telemetry
{
    public static class SpellDiagnosticsTelemetry
    {
        public const string SourceName = "NexusForever.Game.Spell";
        public const string MeterName = "NexusForever.Game.Spell";

        public static readonly ActivitySource ActivitySource = new(SourceName);
        public static readonly Meter Meter = new(MeterName);
    }
}
