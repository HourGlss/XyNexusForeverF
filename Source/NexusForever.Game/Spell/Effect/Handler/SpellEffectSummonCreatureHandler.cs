using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.SummonCreature)]
    [SpellEffectHandler(SpellEffectType.SummonTrap)]
    public class SpellEffectSummonCreatureHandler : ISpellEffectApplyHandler<ISpellEffectSummonCreatureData>, ISpellEffectRemoveHandler<ISpellEffectSummonCreatureData>
    {
        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public SpellEffectSummonCreatureHandler(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSummonCreatureData data)
        {
            if (data.CreatureId == 0u)
                return SpellEffectExecutionResult.PreventEffect;

            IUnitEntity caster = executionContext.Spell.Caster;
            if (caster.Map == null)
                return SpellEffectExecutionResult.PreventEffect;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(data.CreatureId);
            if (creatureInfo == null)
                return SpellEffectExecutionResult.PreventEffect;

            uint count = Math.Max(data.Count, 1u);
            for (uint i = 0; i < count; i++)
                caster.SummonFactory.Summon(creatureInfo, GetSummonPosition(caster, target, data), caster.Rotation);

            return SpellEffectExecutionResult.Ok;
        }

        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSummonCreatureData data)
        {
            if (info.Entry.DurationTime == 0u)
                return;

            spell.Caster.SummonFactory.UnsummonCreature(data.CreatureId);
        }

        private static Vector3 GetSummonPosition(IUnitEntity caster, IUnitEntity target, ISpellEffectSummonCreatureData data)
        {
            Vector3 position = target.Position;

            if (data.MaxDistance > 0f)
            {
                float distance = data.MinDistance;
                if (data.MaxDistance > data.MinDistance)
                    distance += (float)Random.Shared.NextDouble() * (data.MaxDistance - data.MinDistance);

                float angle = (float)Random.Shared.NextDouble() * MathF.PI * 2f;
                position = target.Position.GetPoint2D(angle, distance);
            }

            float? terrainHeight = caster.Map.GetTerrainHeight(position.X, position.Z);
            if (terrainHeight.HasValue && position.Y < terrainHeight.Value)
                position.Y = terrainHeight.Value;

            return position;
        }
    }
}
