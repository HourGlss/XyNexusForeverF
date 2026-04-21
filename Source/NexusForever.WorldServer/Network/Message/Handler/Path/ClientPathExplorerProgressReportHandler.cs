using NexusForever.Game.Entity;
using NexusForever.Game.PathContent;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Path
{
    public class ClientPathExplorerProgressReportHandler : IMessageHandler<IWorldSession, ClientPathExplorerProgressReport>
    {
        public void HandleMessage(IWorldSession session, ClientPathExplorerProgressReport progressReport)
        {
            if (session.Player is Player player)
                GlobalPathContentManager.Instance.HandleExplorerPlaceSignal(player, progressReport);
        }
    }
}
