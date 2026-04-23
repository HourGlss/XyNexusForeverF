using System;
using System.IO;
using NexusForever.Shared;

namespace NexusForever.StsServer.Network.Packet
{
    public class FragmentedStsPacket
    {
        public bool HasHeader => packet != null;
        public bool HasBody => packet?.Body != null;

        private readonly byte[] buffer = new byte[2048];
        private uint position;

        private ClientStsPacket packet;
        private uint dataLength;

        public void Populate(BinaryReader reader)
        {
            if (HasHeader)
            {
                uint remaining = reader.BaseStream.Remaining();
                uint remainingBody = dataLength - position;
                uint length = Math.Min(remaining, remainingBody);

                byte[] data = reader.ReadBytes((int)length);
                Buffer.BlockCopy(data, 0, buffer, (int)position, (int)length);
                position += length;

                if (position == dataLength)
                    packet.SetBody(buffer, dataLength);
            }
            else
            {
                while (reader.BaseStream.Remaining() != 0)
                {
                    buffer[position++] = reader.ReadByte();
                    if (position < 4)
                        continue;

                    // end of header is marked by \r\n\r\n
                    if (BitConverter.ToUInt32(buffer, (int)position - 4) == 0x0A0D0A0Du)
                    {
                        packet     = new ClientStsPacket(buffer);
                        position   = 0;
                        dataLength = uint.Parse(packet.Headers["l"]);
                        if (dataLength == 0)
                            packet.SetBody(buffer, 0);

                        break;
                    }
                }
            }
        }

        public ClientStsPacket GetPacket()
        {
            return packet;
        }
    }
}
