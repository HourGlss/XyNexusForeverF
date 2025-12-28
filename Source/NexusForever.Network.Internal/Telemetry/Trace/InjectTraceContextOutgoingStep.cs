using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Rebus.Bus;
using Rebus.Pipeline;

namespace NexusForever.Network.Internal.Telemetry.Trace
{
    public class InjectTraceContextOutgoingStep : IOutgoingStep
    {
        public async Task Process(OutgoingStepContext context, Func<Task> next)
        {
            var message = context.Load<Rebus.Messages.Message>();

            using Activity activity = TraceStatic.Messaging.StartActivity($"Send Internal Message {message.GetMessageType()}", ActivityKind.Producer);
            activity?.AddTag("Type", message.GetMessageType());

            // inject parent trace context into message headers
            var parent = new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current);
            Propagators.DefaultTextMapPropagator.Inject(parent, message.Headers,
                (headers, key, value) => headers[key] = value);

            await next();
        }
    }
}
