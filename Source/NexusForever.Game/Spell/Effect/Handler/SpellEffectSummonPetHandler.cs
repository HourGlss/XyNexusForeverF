using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.SummonPet)]
    public class SpellEffectSummonPetHandler : ISpellEffectApplyHandler<ISpellEffectSummonPetData>, ISpellEffectRemoveHandler<ISpellEffectSummonPetData>
    {
        #region Dependency Injection

        private readonly IEntityFactory entityFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        public SpellEffectSummonPetHandler(
            IEntityFactory entityFactory,
            ICreatureInfoManager creatureInfoManager)
        {
            this.entityFactory       = entityFactory;
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSummonPetData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (player.Map == null)
                return SpellEffectExecutionResult.PreventEffect;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(data.CreatureId);
            if (creatureInfo == null)
                return SpellEffectExecutionResult.PreventEffect;

            IPetEntity pet = entityFactory.CreateEntity<IPetEntity>();
            pet.InitialiseCombat(player, creatureInfo, executionContext.Spell.Parameters.SpellInfo.Entry.Id);
            pet.AddToMap(player.Map, player.Position);

            return SpellEffectExecutionResult.Ok;
        }

        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSummonPetData data)
        {
            if (target is not IPlayer player)
                return;

            if (info.Entry.DurationTime == 0u)
                return;

            player.SummonFactory.UnsummonCreature(data.CreatureId);
        }
    }
}
