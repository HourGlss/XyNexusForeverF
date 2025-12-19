using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    [ScriptFilterScriptName("ObjectiveRingEntityScript")]
    public class ObjectiveRingEntityScript : NavigatingNexusSequentialEntityScript, IWorldEntityScript
    {
        private enum SpellId
        {
            ClothesSpawnIn = 86913
        }

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParameterFactory;

        public ObjectiveRingEntityScript(
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.spellParameterFactory = spellParameterFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public override void OnLoad(IWorldEntity owner)
        {
            base.OnLoad(owner);
            owner.SetInRangeCheck(5f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            uint data = owner.QuestChecklistIdx switch
            {
                0 => 8539,
                1 => 8542,
                2 => 8543,
                _ => 0
            };

            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterArea, data, 1);

            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            player.CastSpell(SpellId.ClothesSpawnIn, parameters);
        }
    }
}
