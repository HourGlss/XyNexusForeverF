using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace NexusForever.Shared
{
    // TODO: Rawaho: this really needs to go away for anything except the WorldServer
    public class WorldManager : IWorldManager
    {
        protected static readonly double TargetTps            = 30.0d;
        protected static readonly TimeSpan TargetTickInterval = TimeSpan.FromSeconds(1d / TargetTps);

        private Thread worldThread;
        protected readonly ManualResetEventSlim waitHandle = new();

        protected volatile CancellationTokenSource cancellationToken;

        #region Dependency Injection

        protected readonly ILogger<WorldManager> log;

        public WorldManager(
            ILogger<WorldManager> log)
        {
            this.log = log;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="IWorldManager"/> and any related resources.
        /// </summary>
        public void Initialise(Action<double> updateAction)
        {
            if (cancellationToken != null)
                throw new InvalidOperationException();

            log.LogInformation("Initialising world manager...");

            cancellationToken = new CancellationTokenSource();

            worldThread = new Thread(() => WorldThread(updateAction));
            worldThread.Start();

            // wait for world thread to start before continuing
            waitHandle.Wait();
        }

        protected virtual void WorldThread(Action<double> updateAction)
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

                if (elapsed < TargetTickInterval)
                    Thread.Sleep(TargetTickInterval - elapsed);

                delta = Stopwatch.GetElapsedTime(tickStart, Stopwatch.GetTimestamp());
            }

            log.LogInformation("Stopped world thread.");
        }

        /// <summary>
        /// Request shutdown of <see cref="IWorldManager"/> and any related resources.
        /// </summary>
        public void Shutdown()
        {
            if (cancellationToken == null)
                throw new InvalidOperationException();

            log.LogInformation("Shutting down world manager...");

            cancellationToken.Cancel();

            worldThread.Join();
            worldThread = null;

            cancellationToken = null;
        }
    }
}
