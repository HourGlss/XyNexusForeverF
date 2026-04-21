using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    internal static class SpellEffectCooldownHelper
    {
        public static bool TryApplyCooldown(IPlayer player, ISpellInfoManager spellInfoManager, SpellEffectModifySpellCooldownType type, uint data, double cooldown, ILogger log)
        {
            if (cooldown < 0d)
                return false;

            switch (type)
            {
                case SpellEffectModifySpellCooldownType.SpellBase:
                    return TryApplyBaseSpellCooldown(player, spellInfoManager, data, cooldown);
                case SpellEffectModifySpellCooldownType.Spell4:
                    if (spellInfoManager.GetSpellInfo(data) == null)
                        return false;

                    player.SpellManager.SetSpellCooldown(data, cooldown, true);
                    return true;
                case SpellEffectModifySpellCooldownType.SpellCooldownId:
                    if (data == 0u)
                        return false;

                    player.SpellManager.SetSpellCooldownByCooldownId(data, cooldown);
                    return true;
                default:
                    log.LogWarning("Unhandled spell cooldown selector {Type} for data {Data}.", type, data);
                    return false;
            }
        }

        public static bool TryReadSelector(ISpellEffectDefaultData data, ISpell spell, out SpellEffectModifySpellCooldownType type, out uint value)
        {
            type  = (SpellEffectModifySpellCooldownType)data.DataBits00;
            value = data.DataBits01;

            if (data.DataBits00 == 0u && data.DataBits01 == 0u)
            {
                type  = SpellEffectModifySpellCooldownType.Spell4;
                value = spell.Parameters.SpellInfo.Entry.Id;
            }

            return value != 0u;
        }

        public static double ReadCooldownSeconds(ISpellEffectDefaultData data, double fallback)
        {
            float seconds = BitConverter.Int32BitsToSingle(unchecked((int)data.DataBits02));
            if (float.IsNormal(seconds) && seconds > 0f)
                return seconds;

            if (data.DataBits02 > 0u)
                return data.DataBits02 / 1000d;

            return fallback;
        }

        private static bool TryApplyBaseSpellCooldown(IPlayer player, ISpellInfoManager spellInfoManager, uint spell4BaseId, double cooldown)
        {
            if (spell4BaseId == 0u)
                return false;

            ICharacterSpell characterSpell = player.SpellManager.GetSpell(spell4BaseId);
            if (characterSpell != null)
            {
                player.SpellManager.SetSpellCooldown(characterSpell.SpellInfo, cooldown, true);
                return true;
            }

            ISpellBaseInfo baseInfo = spellInfoManager.GetSpellBaseInfo(spell4BaseId);
            if (baseInfo == null)
                return false;

            bool applied = false;
            foreach (ISpellInfo spellInfo in baseInfo)
            {
                player.SpellManager.SetSpellCooldown(spellInfo, cooldown, true);
                applied = true;
            }

            return applied;
        }
    }
}
