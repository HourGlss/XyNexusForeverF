using OpenTelemetry;

namespace NexusForever.Network.Telemetry
{
    public static class OpenTelemetryBuilderExtensions
    {
        public static OpenTelemetryBuilder AddNetworkTracing(this OpenTelemetryBuilder builder)
        {
            builder.WithTracing(t => t.AddSource("NexusForever.Network"));
            return builder;
        }
    }
}
