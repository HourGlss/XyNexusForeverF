using OpenTelemetry;
using OpenTelemetry.Trace;

namespace NexusForever.API.Telemetry
{
    public static class OpenTelemetryBuilderExtensions
    {
        public static OpenTelemetryBuilder AddAspTracing(this OpenTelemetryBuilder builder)
        {
            builder.WithTracing(t => t.AddAspNetCoreInstrumentation());
            return builder;
        }

        public static OpenTelemetryBuilder AddHttpClientTracing(this OpenTelemetryBuilder builder)
        {
            builder.WithTracing(t => t.AddHttpClientInstrumentation());
            return builder;
        }
    }
}
