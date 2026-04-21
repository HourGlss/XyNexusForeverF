using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.Pet;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.UnlockVanityPet)]
    public class SpellEffectUnlockVanityPetHandler : ISpellEffectApplyHandler<ISpellEffectUnlockVanityPetData>
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public SpellEffectUnlockVanityPetHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectUnlockVanityPetData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            Spell4Entry spell4Entry = gameTableManager.Spell4.GetEntry(data.SpellId);
            if (spell4Entry == null || spell4Entry.Spell4BaseIdBaseSpell == 0)
                return SpellEffectExecutionResult.PreventEffect;

            if (player.SpellManager.GetSpell(spell4Entry.Spell4BaseIdBaseSpell) != null)
                return SpellEffectExecutionResult.Ok;

            player.SpellManager.AddSpell(spell4Entry.Spell4BaseIdBaseSpell);

            player.Session.EnqueueMessageEncrypted(new ServerUnlockVanityPet
            {
                Spell4Id = spell4Entry.Id
            });

            return SpellEffectExecutionResult.Ok;
        }
    }
}
