using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;

namespace NexusForever.WorldServer.Telemetry.Metric
{
    public static class OpenTelemetryBuilderExtensions
    {
        public static OpenTelemetryBuilder AddWorldServerMetrics(this OpenTelemetryBuilder builder)
        {
            builder.WithMetrics(m => m.AddMeter(MetricStatic.MeterName));

            builder.Services
                .AddTransient<WorldManagerTickMetric>()
                .AddTransient<WorldManagerTickMissMetric>();

            return builder;
        }
    }
}
