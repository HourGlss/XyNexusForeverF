using NexusForever.Game.Entity;
using NexusForever.Game.PathContent;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Path
{
    public class ClientPathSettlerImprovementBuildTierHandler : IMessageHandler<IWorldSession, ClientPathSettlerImprovementBuildTier>
    {
        public void HandleMessage(IWorldSession session, ClientPathSettlerImprovementBuildTier buildTier)
        {
            if (session.Player is Player player)
                GlobalPathContentManager.Instance.HandleSettlerBuildImprovement(player, buildTier);
        }
    }
}
