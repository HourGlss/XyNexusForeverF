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
    [SpellEffectHandler(SpellEffectType.ModifyInterruptArmour)]
    public class SpellEffectModifyInterruptArmourHandler : ISpellEffectApplyHandler<ISpellEffectModifyInterruptArmourData>, ISpellEffectRemoveHandler<ISpellEffectModifyInterruptArmourData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectModifyInterruptArmourData data)
        {
            if (target.MaxInterruptArmour == -1)
                return SpellEffectExecutionResult.Ok;

            target.CrowdControlManager.AddTemporaryInterruptArmour(info, data.InterruptArmour);

            executionContext.AddCombatLog(new CombatLogModifyInterruptArmor
            {
                Amount = data.InterruptArmour,
                CastData = new CombatLogCastData
                {
                    CasterId = executionContext.Spell.Caster.Guid,
                    TargetId = target.Guid,
                    SpellId = executionContext.Spell.Spell4Id,
                    CombatResult = CombatResult.Hit
                }
            });

            return SpellEffectExecutionResult.Ok;
        }

        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectModifyInterruptArmourData data)
        {
            target.CrowdControlManager.RemoveTemporaryInterruptArmour(info);
        }
    }
}
