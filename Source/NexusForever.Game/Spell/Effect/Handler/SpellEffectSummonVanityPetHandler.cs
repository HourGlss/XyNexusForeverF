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
    [SpellEffectHandler(SpellEffectType.SummonVanityPet)]
    public class SpellEffectSummonVanityPetHandler : ISpellEffectApplyHandler<ISpellEffectSummonVanityPetData>
    {
        #region Dependency Injection

        private readonly IEntityFactory entityFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        public SpellEffectSummonVanityPetHandler(
            IEntityFactory entityFactory,
            ICreatureInfoManager creatureInfoManager)
        {
            this.entityFactory       = entityFactory;
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSummonVanityPetData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(data.CreatureId);
            if (creatureInfo == null)
                return SpellEffectExecutionResult.PreventEffect;

            // enqueue removal of existing vanity pet if summoned
            if (player.VanityPetGuid != null)
            {
                IPetEntity oldVanityPet = player.GetVisible<IPetEntity>(player.VanityPetGuid.Value);
                oldVanityPet?.RemoveFromMap();
                player.VanityPetGuid = 0u;
            }

            var pet = entityFactory.CreateEntity<IPetEntity>();
            pet.Initialise(player, creatureInfo);
            pet.AddToMap(player.Map, player.Position);

            return SpellEffectExecutionResult.Ok;
        }
    }
}
