using Microsoft.Extensions.Logging;

namespace Fargo.Infrastructure.Outbox;

/// <summary>
/// Source-generated log messages of the outbox relay.
/// </summary>
internal static partial class OutboxLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Outbox published {Published} of {Claimed} claimed messages.")]
    internal static partial void OutboxBatchPublished(this ILogger logger, int published, int claimed);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox failed to publish event {EventId} ({EventType}), attempt {Attempts}.")]
    internal static partial void OutboxPublishFailed(this ILogger logger, Exception exception, Guid eventId, string eventType, int attempts);

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox relay failed; retrying in {Delay}.")]
    internal static partial void OutboxRelayFailed(this ILogger logger, Exception exception, TimeSpan delay);

    [LoggerMessage(Level = LogLevel.Information, Message = "Outbox purged {Deleted} messages published before {Cutoff}.")]
    internal static partial void OutboxPurged(this ILogger logger, int deleted, DateTimeOffset cutoff);
}
