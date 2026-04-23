using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Prerequisite.Check
{
    public abstract class PrerequisiteCheckSpellCountBase : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        private const uint EngineerBotLimiterSpell4Id = 56487;

        private readonly PrerequisiteType type;

        protected PrerequisiteCheckSpellCountBase(
            ILogger<BasePrerequisiteHandler> log,
            PrerequisiteType type)
            : base(log)
        {
            this.type = type;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint spellCount = CountActiveSpell(player, objectId, parameters);
            return MatchComparable(spellCount, value, comparison, type);
        }

        private static uint CountActiveSpell(IPlayer player, uint spell4Id, IPrerequisiteParameters parameters)
        {
            uint activeSpellCount = player.CountSpells(s => s.Spell4Id == spell4Id);
            if (spell4Id != EngineerBotLimiterSpell4Id)
                return activeSpellCount;

            uint combatPetCount = (uint)player.SummonFactory.GetSummons<IPetEntity>().Count(p => p.IsCombatPet);
            uint currentCount = combatPetCount;

            return CurrentSpellSummonsPet(parameters) ? currentCount + 1u : currentCount;
        }

        private static bool CurrentSpellSummonsPet(IPrerequisiteParameters parameters)
        {
            return parameters.SpellInfo?.Effects.Any(e => e.EffectType == SpellEffectType.SummonPet) == true;
        }
    }

    [PrerequisiteCheck(PrerequisiteType.Spell59)]
    public class PrerequisiteCheckSpell59 : PrerequisiteCheckSpellCountBase
    {
        public PrerequisiteCheckSpell59(ILogger<BasePrerequisiteHandler> log)
            : base(log, PrerequisiteType.Spell59)
        {
        }
    }

    [PrerequisiteCheck(PrerequisiteType.Spell60)]
    public class PrerequisiteCheckSpell60 : PrerequisiteCheckSpellCountBase
    {
        public PrerequisiteCheckSpell60(ILogger<BasePrerequisiteHandler> log)
            : base(log, PrerequisiteType.Spell60)
        {
        }
    }
}
