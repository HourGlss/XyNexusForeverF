using OpenTelemetry;

namespace NexusForever.Network.Internal.Telemetry.Trace
{
    public static class OpenTelemetryBuilderExtensions
    {
        public static OpenTelemetryBuilder AddNetworkInternalTracing(this OpenTelemetryBuilder builder)
        {
            builder.WithTracing(t => t.AddSource("NexusForever.Network.Internal"));
            return builder;
        }
    }
}
