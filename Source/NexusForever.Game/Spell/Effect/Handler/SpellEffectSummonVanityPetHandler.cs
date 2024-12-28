using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.SummonVanityPet)]
    public class SpellEffectSummonVanityPetHandler : ISpellEffectApplyHandler<ISpellEffectSummonVanityPetData>
    {
        #region Dependency Injection

        private readonly IEntityFactory entityFactory;

        public SpellEffectSummonVanityPetHandler(
            IEntityFactory entityFactory)
        {
            this.entityFactory = entityFactory;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSummonVanityPetData data)
        {
            if (target is not IPlayer player)
                return;

            // enqueue removal of existing vanity pet if summoned
            if (player.VanityPetGuid != null)
            {
                IPetEntity oldVanityPet = player.GetVisible<IPetEntity>(player.VanityPetGuid.Value);
                oldVanityPet?.RemoveFromMap();
                player.VanityPetGuid = 0u;
            }

            var pet = entityFactory.CreateEntity<IPetEntity>();
            pet.Initialise(player, data.CreatureId);
            pet.AddToMap(player.Map, player.Position);
        }
    }
}
