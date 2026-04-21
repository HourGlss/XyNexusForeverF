using NexusForever.Game;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.GiveItemToPlayer)]
    public class SpellEffectGiveItemToPlayerHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits00 == 0u || ItemManager.Instance.GetItemInfo(data.DataBits00) == null)
                return SpellEffectExecutionResult.PreventEffect;

            uint count   = data.DataBits01 == 0u ? 1u : data.DataBits01;
            uint charges = data.DataBits02;
            player.Inventory.ItemCreate(InventoryLocation.Inventory, data.DataBits00, count, ItemUpdateReason.SpellEffect, charges);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
