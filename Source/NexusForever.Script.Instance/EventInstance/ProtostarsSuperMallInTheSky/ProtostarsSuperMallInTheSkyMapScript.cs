using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.EventInstances.ProtostarsSuperMallInTheSky
{
    [ScriptFilterOwnerId(3094)]
    public class ProtostarsSuperMallInTheSkyMapScript : EventBaseContentMapScript
    {
        public override uint PublicEventId => 679u;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public ProtostarsSuperMallInTheSkyMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to map.
        /// </summary>
        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is not IPlayer player)
                return;

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic< IProtostarSuperMallInTheSkyOnCreate > ());
        }
    }
}
