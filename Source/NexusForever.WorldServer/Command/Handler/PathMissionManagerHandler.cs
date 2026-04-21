using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.RBAC;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Static;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Path, "A collection of commands to manage path missions for a character.", "pathmission")]
    [CommandTarget(typeof(IPlayer))]
    public class PathMissionManagerHandler : CommandCategory
    {
        [Command(Permission.Path, "Create a path episode for character.", "episodeadd")]
        public void HandleEpisodeAdd(ICommandContext context,
            [Parameter("Path episode id.")]
            uint episodeId)
        {
            if (context.GetTargetOrInvoker<IPlayer>() is not Player player)
                return;

            var episode = player.PathMissionManager.PathEpisodeCreate(episodeId);
            if (episode == null)
            {
                context.SendMessage($"Unknown episode: {episodeId}");
                return;
            }

            context.SendMessage($"Created path episode {episode.Id}.");
        }

        [Command(Permission.Path, "Unlock a path mission for character.", "missionunlock")]
        public void HandleMissionUnlock(ICommandContext context,
            [Parameter("Path mission id.")]
            uint missionId)
        {
            if (context.GetTargetOrInvoker<IPlayer>() is not Player player)
                return;

            player.PathMissionManager.UnlockMission(missionId);
            context.SendMessage($"Unlocked mission {missionId}.");
        }
    }
}
