using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.DeepSpaceExploration
{
    [ScriptFilterOwnerId(2188)]
    public class DeepSpaceExplorationMapScript : EventBaseContentMapScript
    {
        public override uint PublicEventId => 447u;

        private readonly ICinematicFactory cinematicFactory;

        public DeepSpaceExplorationMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is not IPlayer player)
                return;

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IDeepSpaceExplorationOnCreate>());
        }
    }
}
