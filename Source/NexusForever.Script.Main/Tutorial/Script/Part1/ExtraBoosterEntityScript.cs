using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    [ScriptFilterCreatureId(73461)]
    public class ExtraBoosterEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private enum SpellId
        {
            ExtraGas = 85483
        }

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParameterFactory;

        public ExtraBoosterEntityScript(
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.spellParameterFactory = spellParameterFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            owner.SetInRangeCheck(5f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            var parameters = spellParameterFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            player.CastSpell(SpellId.ExtraGas, parameters);
        }
    }
}
