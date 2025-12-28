using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using Rebus.Bus;
using Rebus.Pipeline;

namespace NexusForever.Network.Internal.Telemetry.Trace
{
    public class ExtractTraceContextIncomingStep : IIncomingStep
    {
        public async Task Process(IncomingStepContext context, Func<Task> next)
        {
            var message = context.Load<Rebus.Messages.Message>();

            // extract parent trace context from message headers
            PropagationContext parent = Propagators.DefaultTextMapPropagator.Extract(default, message.Headers,
                (headers, key) => headers.TryGetValue(key, out string value) ? new[] { value } : []);

            using Activity activity = TraceStatic.Messaging.StartActivity($"Receive Internal Message {message.GetMessageType()}", ActivityKind.Consumer, parent.ActivityContext);
            activity?.SetTag("Type", message.GetMessageType());

            await next();
        }
    }
}
