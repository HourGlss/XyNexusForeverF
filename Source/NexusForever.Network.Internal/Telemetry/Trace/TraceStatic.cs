using System.Diagnostics;

namespace NexusForever.Network.Internal.Telemetry.Trace
{
    public static class TraceStatic
    {
        public static readonly ActivitySource Messaging = new("NexusForever.Network.Internal");
    }
}
