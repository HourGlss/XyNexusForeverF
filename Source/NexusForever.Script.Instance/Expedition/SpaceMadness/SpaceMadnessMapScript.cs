using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.SpaceMadness
{
    [ScriptFilterOwnerId(2149)]
    public class SpaceMadnessMapScript : EventBaseContentMapScript
    {
        public override uint PublicEventId => 390u;

        private readonly ICinematicFactory cinematicFactory;

        public SpaceMadnessMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is not IPlayer player)
                return;

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<ISpaceMadnessOnCreate>());
        }
    }
}
