using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Telemetry.Configuration.Model;
using NexusForever.Telemetry.Metric;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NexusForever.Telemetry
{
    public static class ServiceCollectionExtensions
    {
        public static OpenTelemetryBuilder AddNexusForeverTelemetry(this IServiceCollection sc, IConfigurationSection configurationSection)
        {
            sc.AddSingleton<IMeticFactory, MetricFactory>();

            TelemetryOptions options = configurationSection.Get<TelemetryOptions>();
            if (options == null)
                return null;

            sc.AddOptions<TelemetryOptions>()
                .Bind(configurationSection);

            OpenTelemetryBuilder otb = sc.AddOpenTelemetry()
                .ConfigureResource(r => r.AddService(GetServiceName(options)));

            if (options.Logging?.Enable is true)
                otb.WithLogging(l => l.AddOtlpExporter(e => e.AddNexusForeverOtlpExporter(options.Endpoint, "v1/logs")));

            if (options.Metrics?.Enable is true)
                otb.WithMetrics(m => m.AddOtlpExporter(e => e.AddNexusForeverOtlpExporter(options.Endpoint, "v1/metrics")));

            if (options.Tracing?.Enable is true)
                otb.WithTracing(t => t.AddOtlpExporter(e => e.AddNexusForeverOtlpExporter(options.Endpoint, "v1/traces")));

            return otb;
        }

        private static string GetServiceName(TelemetryOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.ServiceName))
                return options.ServiceName;

            string environmentServiceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
            if (!string.IsNullOrWhiteSpace(environmentServiceName))
                return environmentServiceName;

            return Assembly.GetEntryAssembly()?.GetName().Name
                ?? AppDomain.CurrentDomain.FriendlyName
                ?? "NexusForever";
        }
    }
}
