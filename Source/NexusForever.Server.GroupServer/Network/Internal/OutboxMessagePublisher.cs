using System.Diagnostics;
using System.Text.Json;
using NexusForever.Database.Group.Model;
using NexusForever.Database.Group.Repository;
using NexusForever.Network.Internal;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace NexusForever.Server.GroupServer.Network.Internal
{
    public class OutboxMessagePublisher : IInternalMessagePublisher
    {
        #region Dependency Injection

        private readonly InternalMessageRepository _repository;

        public OutboxMessagePublisher(
            InternalMessageRepository repository)
        {
            _repository = repository;
        }

        #endregion

        /// <summary>
        /// Publish a message to the internal message broker via an outbox table.
        /// </summary>
        /// <param name="message"></param>
        public async Task PublishAsync(object message)
        {
            using Activity activity = NexusForever.Network.Internal.Telemetry.Trace.TraceStatic.Messaging.StartActivity($"Send Outbox Message {message.GetType()}", ActivityKind.Producer);
            activity?.SetTag("Type", message.GetType());

            var values = new Dictionary<string, string>();
            var parent = new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current);
            Propagators.DefaultTextMapPropagator.Inject(parent, values,
                (headers, key, value) => headers[key] = value);

            _repository.AddMessage(new InternalMessageModel
            {
                Id        = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Type      = message.GetType().AssemblyQualifiedName,
                Payload   = await SerialisePayload(message),
                Metadata  = await SerialiseMetadata(values)
            });
        }

        private async Task<string> SerialiseMetadata(Dictionary<string, string> values)
        {
            using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);
            await JsonSerializer.SerializeAsync(stream, values);

            stream.Position = 0;
            return await reader.ReadToEndAsync();
        }

        private async Task<string> SerialisePayload(object message)
        {
            using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);
            await JsonSerializer.SerializeAsync(stream, message, message.GetType());

            stream.Position = 0;
            return await reader.ReadToEndAsync();
        }
    }
}
