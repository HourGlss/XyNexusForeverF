using System.Diagnostics;
using System.Text.Json;
using NexusForever.Database.Chat.Model;
using NexusForever.Database.Chat.Repository;
using NexusForever.Network.Internal;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace NexusForever.Server.ChatServer.Network.Internal.Handler
{
    public class OutboxMessagePublisher : IInternalMessagePublisher
    {
        private readonly List<Guid> _urgentMessages = [];

        #region Dependency Injection

        private readonly InternalMessageRepository _repository;
        private readonly OutboxUrgentSignal _outboxSignal;

        public OutboxMessagePublisher(
            InternalMessageRepository repository,
            OutboxUrgentSignal outboxSignal)
        {
            _repository = repository;
            _outboxSignal = outboxSignal;
        }

        #endregion

        /// <summary>
        /// Publish a message to the internal message broker via an outbox table.
        /// </summary>
        /// <param name="message">Message to publish.</param>
        public async Task PublishAsync(object message)
        {
            using Activity activity = NexusForever.Network.Internal.Telemetry.Trace.TraceStatic.Messaging.StartActivity($"Send Outbox Message {message.GetType()}", ActivityKind.Producer);
            activity?.SetTag("Type", message.GetType());

            Dictionary<string, string> values = CreateHeaders();
            InternalMessageModel internalMessage = await CreateMessage(message, values);
            _repository.AddMessage(internalMessage);
        }

        /// <summary>
        /// Publish a message to the internal message broker via an outbox table.
        /// </summary>
        /// <remarks>
        /// The message will be marked as urgent and will be processed immediately.
        /// If the immediate processing fails it will be handled like a normal outbox message.
        /// </remarks>
        /// <param name="message">Message to publish.</param>
        public async Task PublishUrgentAsync(object message)
        {
            using Activity activity = NexusForever.Network.Internal.Telemetry.Trace.TraceStatic.Messaging.StartActivity($"Send Outbox Message {message.GetType()}", ActivityKind.Producer);
            activity?.SetTag("Type", message.GetType());
            activity?.SetTag("Urgent", true);

            Dictionary<string, string> values = CreateHeaders();
            InternalMessageModel internalMessage = await CreateMessage(message, values);
            _repository.AddMessage(internalMessage);
            _urgentMessages.Add(internalMessage.Id);
        }

        private Dictionary<string, string> CreateHeaders()
        {
            var values = new Dictionary<string, string>();
            var parent = new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current);
            Propagators.DefaultTextMapPropagator.Inject(parent, values,
                (headers, key, value) => headers[key] = value);

            return values;
        }

        /// <summary>
        /// Send all urgent messages for immediate processing.
        /// </summary>
        public async Task FlushUrgentMessages()
        {
            foreach (Guid messageGuid in _urgentMessages)
                await _outboxSignal.WriteUrgent(messageGuid);

            _urgentMessages.Clear();
        }

        private async Task<InternalMessageModel> CreateMessage(object message, Dictionary<string, string> headers)
        {
            var model = new InternalMessageModel
            {
                Id        = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Type      = message.GetType().AssemblyQualifiedName,
                Payload   = await SerialisePayload(message),
                Metadata  = await SerialiseMetadata(headers)
            };

            return model;
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
