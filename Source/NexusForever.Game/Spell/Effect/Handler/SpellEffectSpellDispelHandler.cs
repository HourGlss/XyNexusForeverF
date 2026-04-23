using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Network.World.Combat;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.SpellDispel)]
    public class SpellEffectSpellDispelHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            uint removalLimit = GetRemovalLimit(data);
            if (removalLimit == 0u)
                return SpellEffectExecutionResult.Ok;

            SpellClass spellClass = (SpellClass)data.DataBits03;
            ISpell[] spellsToDispel = target.GetSpells(s => IsMatchingDispelTarget(s, spellClass))
                .Take((int)Math.Min(removalLimit, (uint)int.MaxValue))
                .ToArray();

            foreach (ISpell spell in spellsToDispel)
            {
                spell.Finish();
                executionContext.AddCombatLog(new CombatLogDispel
                {
                    BRemovesSingleInstance = data.DataBits04 != 0u,
                    InstancesRemoved       = 1u,
                    SpellRemovedId         = spell.Spell4Id
                });
            }

            return SpellEffectExecutionResult.Ok;
        }

        private static bool IsMatchingDispelTarget(ISpell spell, SpellClass spellClass)
        {
            if (spell.Parameters?.SpellInfo?.BaseInfo?.Entry == null)
                return false;

            return spell.Parameters.SpellInfo.BaseInfo.Entry.SpellClass == spellClass;
        }

        private static uint GetRemovalLimit(ISpellEffectDefaultData data)
        {
            return Math.Max(data.DataBits00, data.DataBits01);
        }
    }
}
