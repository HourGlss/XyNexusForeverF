using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using NexusForever.Telemetry.Metric;
using NexusForever.WorldServer.Telemetry.Metric;

namespace NexusForever.WorldServer
{
    public sealed class WorldManager : Shared.WorldManager
    {
        #region Dependency Injection

        private readonly WorldManagerTickMetric tickMetric;
        private readonly WorldManagerTickMissMetric tickMissMetric;

        public WorldManager(
            ILogger<WorldManager> log,
            IMeticFactory metricFactory)
            : base(log)
        {
            tickMetric     = metricFactory.Resolve<WorldManagerTickMetric>();
            tickMissMetric = metricFactory.Resolve<WorldManagerTickMissMetric>();
        }

        #endregion

        protected override void WorldThread(Action<double> updateAction)
        {
            log.LogInformation("Started world thread.");
            waitHandle.Set();

            TimeSpan delta = TimeSpan.FromMilliseconds(0);

            while (!cancellationToken.IsCancellationRequested)
            {
                long tickStart = Stopwatch.GetTimestamp();

                try
                {
                    updateAction(delta.TotalSeconds);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error during world update.");
                }

                long tickEnd = Stopwatch.GetTimestamp();
                TimeSpan elapsed = Stopwatch.GetElapsedTime(tickStart, tickEnd);

                tickMetric?.RecordTick(elapsed.TotalMilliseconds);

                if (elapsed < TargetTickInterval)
                    Thread.Sleep(TargetTickInterval - elapsed);
                else
                    tickMissMetric?.RecordTickMiss(elapsed - TargetTickInterval);

                delta = Stopwatch.GetElapsedTime(tickStart, Stopwatch.GetTimestamp());
            }

            log.LogInformation("Stopped world thread.");
        }
    }
}
