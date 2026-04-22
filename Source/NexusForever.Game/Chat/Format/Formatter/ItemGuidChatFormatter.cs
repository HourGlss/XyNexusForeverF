using NexusForever.Game.Abstract.Chat.Format;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Internal.Message.Chat.Shared.Format;
using NexusForever.Network.Internal.Message.Chat.Shared.Format.Model;
using NexusForever.Network.World.Chat;
using NexusForever.Network.World.Chat.Model;

namespace NexusForever.Game.Chat.Format.Formatter
{
    public class ItemGuidChatFormatter : IInternalChatFormatter<ChatFormatItemGuid>, INetworkChatFormatter<ChatChannelTextItemGuidFormat>, ILocalChatFormatter<ChatFormatItemGuid>
    {
        public IChatChannelTextFormatModel ToInternal(IPlayer player, ChatFormatItemGuid format)
        {
            IItem item = player.Inventory.GetItem(format.ItemGuid);
            if (item == null)
            {
                return new ChatChannelTextItemGuidFormat
                {
                    ItemGuid = format.ItemGuid
                };
            }

            // TODO: Replace with ItemFull format
            return new ChatChannelTextItemIdFormat
            {
                Item2Id = item.Id
            };
        }

        public IChatFormatModel ToNetwork(ChatChannelTextItemGuidFormat format)
        {
            return new ChatFormatItemGuid
            {
                ItemGuid = format.ItemGuid
            };
        }

        public IChatFormatModel ToLocal(IPlayer player, ChatFormatItemGuid format)
        {
            IItem item = player.Inventory.GetItem(format.ItemGuid);
            if (item == null)
                return format;

            // TODO: Replace with ItemFull format
            return new ChatFormatItemId
            {
                Item2Id = item.Id
            };
        }
    }
}
