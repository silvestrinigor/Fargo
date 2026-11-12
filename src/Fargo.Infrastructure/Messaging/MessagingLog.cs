using Microsoft.Extensions.Logging;

namespace Fargo.Infrastructure.Messaging;

/// <summary>
/// Source-generated log messages of the RabbitMQ messaging components.
/// </summary>
internal static partial class MessagingLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Consuming {BindingKey} events from exchange {Exchange} through queue {Queue}.")]
    internal static partial void ConsumerSubscribed(this ILogger logger, string exchange, string bindingKey, string queue);

    [LoggerMessage(Level = LogLevel.Warning, Message = "RabbitMQ consumer stopped: {Reason}. Resubscribing in {Delay}.")]
    internal static partial void ConsumerChannelClosed(this ILogger logger, string reason, TimeSpan delay);

    [LoggerMessage(Level = LogLevel.Error, Message = "RabbitMQ consumer failed to subscribe; retrying in {Delay}.")]
    internal static partial void ConsumerSubscribeFailed(this ILogger logger, Exception exception, TimeSpan delay);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dropped message {MessageId} with routing key {RoutingKey}: {Reason}.")]
    internal static partial void ConsumerMessageDropped(this ILogger logger, string? messageId, string routingKey, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Handling event {EventId} ({EventType}) failed; the event is skipped.")]
    internal static partial void ConsumerHandlerFailed(this ILogger logger, Exception exception, Guid eventId, string eventType);
}
