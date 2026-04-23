using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.WorldStory.JourneyIntoOMNICore1
{
    [ScriptFilterOwnerId(3045)]
    public class JourneyIntoOMNICore1MapScript : EventBaseContentMapScript
    {
        public override uint PublicEventId => 605u;
        //public override uint PublicSubEventId => 631u; // this doesn't have any objectives

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public JourneyIntoOMNICore1MapScript(
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

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<IJourneyIntoOMNICore1OnCreate>());
        }
    }
}
