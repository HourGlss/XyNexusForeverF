using System.IO;
using System.Text;

namespace NexusForever.StsServer.Network.Packet
{
    public class ClientStsPacket : StsPacket
    {
        public string Method { get; }
        public string Uri { get; }

        public ClientStsPacket(byte[] data)
        {
            using (var reader = new StringReader(Encoding.UTF8.GetString(data)))
            {
                string requestLine = reader.ReadLine();
                if (requestLine == null)
                    throw new InvalidDataException("STS packet contains invalid request line!");

                string[] requestParameters = requestLine.Split(' ');
                if (requestParameters.Length != 3)
                    throw new InvalidDataException("STS packet contains invalid request line!");

                Method   = requestParameters[0];
                Uri      = requestParameters[1];
                Protocol = requestParameters[2];

                while (true)
                {
                    string headerLine = reader.ReadLine();
                    if (headerLine == null)
                        throw new InvalidDataException("STS packet contains an invalid header line!");

                    // empty line between header and body data
                    if (headerLine == string.Empty)
                        break;

                    int separatorIndex = headerLine.IndexOf(':');
                    if (separatorIndex == -1)
                        throw new InvalidDataException("STS packet contains an invalid header line!");

                    string headerName  = headerLine[..separatorIndex].Trim();
                    string headerValue = headerLine[(separatorIndex + 1)..].Trim();

                    if (Headers.ContainsKey(headerName))
                        throw new InvalidDataException("STS packet contains duplicate header!");

                    Headers.Add(headerName, headerValue);
                }

                if (!Headers.ContainsKey("l"))
                    throw new InvalidDataException("STS packet doesn't contain a length header!");
            }
        }

        public void SetBody(byte[] data, uint length)
        {
            Body = Encoding.UTF8.GetString(data, 0, (int)length);
        }
    }
}
