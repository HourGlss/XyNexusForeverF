using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Tutorial
{
    [ScriptFilterOwnerId(3460)]
    public class TutorialMapScript : IInstancedMapScript, IOwnedScript<ITutorialMapInstance>
    {
        private enum SpellId
        {
            HoloTechVfx = 86985
        }

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IFactory<ISpellParameters> spellParameterFactory;

        public TutorialMapScript(
            ICinematicFactory cinematicFactory,
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.cinematicFactory      = cinematicFactory;
            this.spellParameterFactory = spellParameterFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to map.
        /// </summary>
        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<INoviceTutorialOnEnter>());

            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            player.CastSpell(SpellId.HoloTechVfx, parameters);

            QuestId questId = player.Faction2 == Faction.Exile
                ? QuestId.NavigatingNexusExile
                : QuestId.NavigatingNexusDominion;

            if (player.QuestManager.GetQuestState(questId) != null)
                return;
            
            player.QuestManager.QuestAdd(questId);
        }
    }
}
