using NexusForever.Telemetry.Configuration.Model;
using OpenTelemetry.Exporter;

namespace NexusForever.Telemetry
{
    public static class OtlpExporterExtensions
    {
        public static void AddNexusForeverOtlpExporter(this OtlpExporterOptions exporterOptions, TelemetryEndpointOptions endpointOptions)
        {
            exporterOptions.Endpoint = new Uri(endpointOptions.Url);
            exporterOptions.Protocol = endpointOptions.Protocol;
            exporterOptions.Headers  = endpointOptions.Headers;
        }
    }
}
