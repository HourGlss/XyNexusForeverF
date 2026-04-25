using NexusForever.Telemetry;
using NexusForever.Telemetry.Configuration.Model;
using OpenTelemetry.Exporter;

namespace NexusForever.Tests;

public class TelemetryTests
{
    [Fact]
    public void HttpProtobufExporterUsesSignalPath()
    {
        var options = new OtlpExporterOptions();

        options.AddNexusForeverOtlpExporter(new TelemetryEndpointOptions
        {
            Url      = "http://127.0.0.1:4318",
            Protocol = OtlpExportProtocol.HttpProtobuf,
            Headers  = ""
        }, "v1/metrics");

        Assert.Equal(new Uri("http://127.0.0.1:4318/v1/metrics"), options.Endpoint);
        Assert.Equal(OtlpExportProtocol.HttpProtobuf, options.Protocol);
    }

    [Fact]
    public void HttpProtobufExporterKeepsConfiguredPath()
    {
        var options = new OtlpExporterOptions();

        options.AddNexusForeverOtlpExporter(new TelemetryEndpointOptions
        {
            Url      = "http://127.0.0.1:4318/custom",
            Protocol = OtlpExportProtocol.HttpProtobuf,
            Headers  = ""
        }, "v1/metrics");

        Assert.Equal(new Uri("http://127.0.0.1:4318/custom"), options.Endpoint);
    }
}
