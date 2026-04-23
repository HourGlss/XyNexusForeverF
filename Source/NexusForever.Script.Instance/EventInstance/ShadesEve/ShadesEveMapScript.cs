using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template.Filter;
using NexusForever.Game.Abstract.Cinematic.Cinematics;

namespace NexusForever.Script.Instance.EventInstance.ShadesEve
{
    [ScriptFilterOwnerId(3044)]
    public class ShadesEveMapScript : EventBaseContentMapScript
    {
        public override uint PublicEventId => 597u;
        //627 Option 1
        //630 Option 2
        //632 Option 1
        //633 Option 2

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public ShadesEveMapScript(
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

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IShadesEveOnCreate>());
        }
    }
}
