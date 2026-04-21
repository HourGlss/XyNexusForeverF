using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.AddSpell)]
    public class SpellEffectAddSpellHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        private readonly IGameTableManager gameTableManager;
        private readonly ISpellInfoManager spellInfoManager;

        public SpellEffectAddSpellHandler(
            IGameTableManager gameTableManager,
            ISpellInfoManager spellInfoManager)
        {
            this.gameTableManager = gameTableManager;
            this.spellInfoManager = spellInfoManager;
        }

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            uint spellId = data.DataBits00;
            if (spellId == 0u)
                return SpellEffectExecutionResult.PreventEffect;

            Spell4Entry spell4Entry = gameTableManager.Spell4.GetEntry(spellId);
            uint spell4BaseId       = spell4Entry?.Spell4BaseIdBaseSpell ?? spellId;
            byte tier               = (byte)Math.Clamp(spell4Entry?.TierIndex ?? data.DataBits01, 1u, byte.MaxValue);

            ISpellBaseInfo spellBaseInfo = spellInfoManager.GetSpellBaseInfo(spell4BaseId);
            if (spellBaseInfo == null || spellBaseInfo.GetSpellInfo(tier) == null)
                return SpellEffectExecutionResult.PreventEffect;

            if (player.SpellManager.GetSpell(spell4BaseId) != null)
                return SpellEffectExecutionResult.Ok;

            player.SpellManager.AddSpell(spell4BaseId, tier);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
