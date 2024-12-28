using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NexusForever.GameTable;

namespace NexusForever.WorldServer.Service
{
    public class GameTableHostedService : IHostedService
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public GameTableHostedService(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await gameTableManager.Initialise();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
