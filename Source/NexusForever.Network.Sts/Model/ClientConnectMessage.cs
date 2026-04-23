using System.Xml;

namespace NexusForever.Network.Sts.Model
{
    [Message("/Sts/Connect")]
    public class ClientConnectMessage : IReadable
    {
        public uint ConnType { get; private set; }
        public string Address { get; private set; }
        public uint? ConnProductType { get; private set; }
        public uint? ConnAppIndex { get; private set; }
        public uint? ConnDeployment { get; private set; }
        public uint? ConnEpoch { get; private set; }
        public uint ProductType { get; private set; }
        public uint AppIndex { get; private set; }
        public uint? Deployment { get; private set; }
        public uint Epoch { get; private set; }
        public uint Program { get; private set; }
        public uint Build { get; private set; }
        public uint Process { get; private set; }
        public uint? NotifyFlags { get; private set; }
        public uint? VersionFlags { get; private set; }

        public void Read(XmlDocument document)
        {
            XmlNode rootNode = document["Connect"];
            if (rootNode == null)
                return;

            ConnType = rootNode["ConnType"].GetValue<uint>();
            Process  = rootNode["Process"].GetValue<uint>();

            ProductType  = rootNode["ProductType"].GetValue<uint>();
            AppIndex     = rootNode["AppIndex"].GetValue<uint>();
            Deployment   = GetOptionalUInt(rootNode["Deployment"]);
            NotifyFlags  = GetOptionalUInt(rootNode["NotifyFlags"]);
            VersionFlags = GetOptionalUInt(rootNode["VersionFlags"]);

            ConnProductType = GetOptionalUInt(rootNode["ConnProductType"]);
            ConnAppIndex    = GetOptionalUInt(rootNode["ConnAppIndex"]);
            ConnDeployment  = GetOptionalUInt(rootNode["ConnDeployment"]);
            ConnEpoch       = GetOptionalUInt(rootNode["ConnEpoch"]);

            Address = rootNode["Address"].GetValue<string>();
            Program = rootNode["Program"].GetValue<uint>();
            Build   = rootNode["Build"].GetValue<uint>();
            Epoch   = rootNode["Epoch"].GetValue<uint>();

            string location = rootNode["Location"].GetValue<string>();
            if (!string.IsNullOrWhiteSpace(location))
                ReadLocation(location);
        }

        public void ReadHeaders(IReadOnlyDictionary<string, string> headers)
        {
            if (headers.TryGetValue("X-Sts-Connect", out string connectHeader))
            {
                ReadXStsConnect(connectHeader);
                return;
            }

            if (headers.ContainsKey("Authorization"))
                ConnType = 301u;
            else if (ConnType == 0u)
                ConnType = 300u;
        }

        private static uint? GetOptionalUInt(XmlNode node)
        {
            if (node == null)
                return null;

            string value = node.InnerText;
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return uint.TryParse(value, out uint parsed) ? parsed : null;
        }

        private void ReadLocation(string location)
        {
            string[] parts = location.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
                return;

            Address = parts[0];

            if (parts.Length > 1 && uint.TryParse(parts[1], out uint program))
                Program = program;
            if (parts.Length > 2 && uint.TryParse(parts[2], out uint build))
                Build = build;
            if (parts.Length > 3 && uint.TryParse(parts[3], out uint epoch))
                Epoch = epoch;
        }

        private void ReadXStsConnect(string connectHeader)
        {
            foreach (string token in connectHeader.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                int separatorIndex = token.IndexOf('=');
                if (separatorIndex == -1)
                {
                    if (uint.TryParse(token, out uint connType))
                        ConnType = connType;

                    continue;
                }

                string name  = token[..separatorIndex].Trim();
                string value = token[(separatorIndex + 1)..].Trim();

                switch (name.ToLowerInvariant())
                {
                    case "product":
                        ProductType = ParseUInt(value);
                        break;
                    case "index":
                        AppIndex = ParseUInt(value);
                        break;
                    case "address":
                        Address = value;
                        break;
                    case "deployment":
                        Deployment = ParseUInt(value);
                        break;
                    case "epoch":
                        Epoch = ParseUInt(value);
                        break;
                    case "notify":
                        NotifyFlags = ParseUInt(value);
                        break;
                    case "version":
                        VersionFlags = ParseUInt(value);
                        break;
                    case "process":
                        Process = ParseUInt(value);
                        break;
                    case "program":
                        Program = ParseUInt(value);
                        break;
                    case "build":
                        Build = ParseUInt(value);
                        break;
                }
            }
        }

        private static uint ParseUInt(string value)
        {
            return uint.TryParse(value, out uint parsed) ? parsed : 0u;
        }
    }
}
