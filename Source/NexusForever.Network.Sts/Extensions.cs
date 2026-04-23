using System.Xml;

namespace NexusForever.Network.Sts
{
    public static class Extensions
    {
        public static T GetValue<T>(this XmlNode node)
        {
            if (node == null)
                return default;

            if (node.NodeType != XmlNodeType.Element)
                return default;

            XmlNode valueNode = node.FirstChild;
            if (valueNode == null)
                return default;

            return (T)Convert.ChangeType(valueNode.Value, typeof(T));
        }
    }
}
