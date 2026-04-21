using NexusForever.Game.Static.Account;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.Shared
{
    public class StorefrontPurchase : IReadable
    {
        public uint OfferId { get; private set; }
        public AccountCurrencyType CurrencyType { get; private set; }
        public float Cost { get; private set; }
        public ushort CurrencyId { get; private set; }
        public uint Unknown1 { get; private set; }
        public Identity Target { get; } = new();
        public uint Unknown2 { get; private set; }

        public void Read(GamePacketReader reader)
        {
            OfferId      = reader.ReadUInt();
            CurrencyType = reader.ReadEnum<AccountCurrencyType>(5u);
            Cost         = reader.ReadSingle();
            CurrencyId   = reader.ReadUShort(14u);
            Unknown1     = reader.ReadUInt();
            Target.Read(reader);
            Unknown2     = reader.ReadUInt();
        }
    }
}
