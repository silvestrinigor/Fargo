using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

internal static partial class PartitionCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Partition create flow started by actor {actorId}.")]
    public static partial void CreateStarted(
        this ILogger logger,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Partition create mutation completed for partition {partitionGuid} by actor {actorId}.")]
    public static partial void CreateCompleted(
        this ILogger logger,
        Guid partitionGuid,
        ActorId actorId);
}
