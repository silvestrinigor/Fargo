namespace Fargo.ServiceMaintenance.Services;

internal static partial class MaintenanceLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Purging expired idempotency keys failed; trying again at the next interval.")]
    public static partial void IdempotencyKeyPurgeFailed(this ILogger logger, Exception exception);
}