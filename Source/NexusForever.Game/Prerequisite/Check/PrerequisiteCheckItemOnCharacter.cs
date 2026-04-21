using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.ItemOnCharacter)]
    public class PrerequisiteCheckItemOnCharacter : BasePrerequisiteHandler, IPrerequisiteCheck
    {
        private static readonly InventoryLocation[] CharacterLocations =
        [
            InventoryLocation.Equipped,
            InventoryLocation.Inventory
        ];

        public PrerequisiteCheckItemOnCharacter(
            ILogger<BasePrerequisiteHandler> log)
            : base(log)
        {
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool hasItem = player.Inventory.HasItem(value, CharacterLocations);
            return MatchBoolean(hasItem, comparison, PrerequisiteType.ItemOnCharacter);
        }
    }
}
