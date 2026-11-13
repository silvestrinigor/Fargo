namespace Fargo.ServiceEventPublisher.Services;

internal static partial class OutboxServiceLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox relay failed; retrying in {Delay}.")]
    internal static partial void OutboxRelayFailed(this ILogger logger, Exception exception, TimeSpan delay);
}
