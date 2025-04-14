using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.UnlockPetFlair)]
    public class SpellEffectUnlockPetFlairHandler : ISpellEffectApplyHandler<ISpellEffectUnlockPetFlairData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectUnlockPetFlairData data)
        {
            if (target is IPlayer player)
                player.PetCustomisationManager.UnlockFlair((ushort)data.PetFlairId);

            return SpellEffectExecutionResult.Ok;
        }
    }
}
