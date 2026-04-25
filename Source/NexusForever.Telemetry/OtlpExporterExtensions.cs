using NexusForever.Telemetry.Configuration.Model;
using OpenTelemetry.Exporter;

namespace NexusForever.Telemetry
{
    public static class OtlpExporterExtensions
    {
        public static void AddNexusForeverOtlpExporter(this OtlpExporterOptions exporterOptions, TelemetryEndpointOptions endpointOptions, string httpProtobufSignalPath = null)
        {
            exporterOptions.Endpoint = GetEndpoint(endpointOptions, httpProtobufSignalPath);
            exporterOptions.Protocol = endpointOptions.Protocol;
            exporterOptions.Headers  = endpointOptions.Headers;
        }

        private static Uri GetEndpoint(TelemetryEndpointOptions endpointOptions, string httpProtobufSignalPath)
        {
            var endpoint = new Uri(endpointOptions.Url);
            if (endpointOptions.Protocol != OtlpExportProtocol.HttpProtobuf || string.IsNullOrWhiteSpace(httpProtobufSignalPath))
                return endpoint;

            if (!string.IsNullOrEmpty(endpoint.AbsolutePath) && endpoint.AbsolutePath != "/")
                return endpoint;

            return new Uri(endpoint, httpProtobufSignalPath);
        }
    }
}
