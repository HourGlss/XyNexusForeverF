using System;
using System.Diagnostics.Metrics;
using NexusForever.Telemetry.Metric;

namespace NexusForever.WorldServer.Telemetry.Metric
{
    public class WorldManagerTickMissMetric : IMetric
    {
        private readonly Counter<int> tickMissCounter;
        private readonly Gauge<double> tickMissGauge;

        public WorldManagerTickMissMetric(
            IMeterFactory meterFactory)
        {
            Meter meter = meterFactory.Create(MetricStatic.MeterName);
            tickMissCounter = meter.CreateCounter<int>("world_manager_tick_miss", description: "Count of world ticks that missed threshold.");
            tickMissGauge   = meter.CreateGauge<double>("world_manager_tick_miss_ms", "ms", "World tick threshold miss in milliseconds.");
        }

        public void RecordTickMiss(TimeSpan span)
        {
            tickMissCounter.Add(1);
            tickMissGauge.Record(span.TotalMilliseconds);
        }
    }
}
