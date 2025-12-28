using System.Diagnostics.Metrics;
using NexusForever.Telemetry.Metric;

namespace NexusForever.WorldServer.Telemetry.Metric
{
    public class WorldManagerTickMetric : IMetric
    {
        private readonly Gauge<double> tickHistogram;

        public WorldManagerTickMetric(
            IMeterFactory meterFactory)
        {
            Meter meter = meterFactory.Create(MetricStatic.MeterName);
            tickHistogram = meter.CreateGauge<double>("world_manager_tick_ms", "ms", "World tick in milliseconds.");
        }

        public void RecordTick(double durationMs)
        {
            tickHistogram.Record(durationMs);
        }
    }
}
