using Microsoft.Extensions.Logging;

namespace Fargo.ServiceOutbox;

/// <summary>
/// Source-generated log messages of the outbox relay host.
/// </summary>
internal static partial class OutboxServiceLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox relay failed; retrying in {Delay}.")]
    internal static partial void OutboxRelayFailed(this ILogger logger, Exception exception, TimeSpan delay);
}
