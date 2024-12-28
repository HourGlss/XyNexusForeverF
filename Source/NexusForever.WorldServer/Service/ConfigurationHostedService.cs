using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NexusForever.Shared.Configuration;

namespace NexusForever.WorldServer.Service
{
    public class ConfigurationHostedService : IHostedService
    {
        #region Dependency Injection

        private readonly ISharedConfiguration sharedConfiguration;

        public ConfigurationHostedService(
            ISharedConfiguration sharedConfiguration)
        {
            this.sharedConfiguration = sharedConfiguration;
        }

        #endregion

        public Task StartAsync(CancellationToken cancellationToken)
        {
            sharedConfiguration.Initialise<WorldServerConfiguration>();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
