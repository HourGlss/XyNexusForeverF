using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.FragmentZero
{
    [ScriptFilterOwnerId(3180)]
    public class FragmentZeroMapScript : EventBaseContentMapScript
    {
        public override uint PublicEventId => 680u;

        private readonly ICinematicFactory cinematicFactory;

        public FragmentZeroMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is not IPlayer player)
                return;

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IFragmentZeroOnCreate>());
        }
    }
}
