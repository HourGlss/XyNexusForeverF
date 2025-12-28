using OpenTelemetry;
using OpenTelemetry.Trace;

namespace NexusForever.Database.Telemetry
{
    public static class OpenTelemetryBuilderExtensions
    {
        public static OpenTelemetryBuilder AddEntityFrameworkTracing(this OpenTelemetryBuilder builder)
        {
            builder.WithTracing(t => t.AddEntityFrameworkCoreInstrumentation());
            return builder;
        }
    }
}
