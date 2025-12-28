namespace NexusForever.Telemetry.Configuration.Model
{
    public class TelemetryOptions
    {
        public TelemetryLoggingOptions Logging { get; set; }
        public TelemetryMetricsOptions Metrics { get; set; }
        public TelemetryTracingOptions Tracing { get; set; }
        public TelemetryEndpointOptions Endpoint { get; set; }
    }
}
