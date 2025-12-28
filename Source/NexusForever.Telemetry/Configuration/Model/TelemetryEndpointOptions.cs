using OpenTelemetry.Exporter;

namespace NexusForever.Telemetry.Configuration.Model
{
    public class TelemetryEndpointOptions
    {
        public string Url { get; set; }
        public OtlpExportProtocol Protocol { get; set; }
        public string Headers { get; set; }
    }
}
