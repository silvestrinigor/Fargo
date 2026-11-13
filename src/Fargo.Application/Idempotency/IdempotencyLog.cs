using Microsoft.Extensions.Logging;

namespace Fargo.Application.Idempotency;

internal static partial class IdempotencyLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Idempotent replay: {Command} for actor {ActorGuid} answered from the stored result.")]
    public static partial void IdempotentReplay(this ILogger logger, string command, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Idempotency keys purged: {Deleted} expired records deleted.")]
    public static partial void IdempotencyKeysPurged(this ILogger logger, int deleted);
}
