namespace Fargo.ServiceMaintenance.Services;

internal static partial class MaintenanceLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Purging expired idempotency keys failed; trying again at the next interval.")]
    public static partial void IdempotencyKeyPurgeFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Purging published outbox messages failed; trying again at the next interval.")]
    public static partial void OutboxPurgeFailed(this ILogger logger, Exception exception);
}
