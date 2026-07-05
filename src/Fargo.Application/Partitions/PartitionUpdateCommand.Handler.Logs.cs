using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

internal static partial class PartitionUpdateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Partition update flow started for partition {partitionGuid} by actor {actorId}.")]
    public static partial void UpdateStarted(
        this ILogger logger,
        Guid partitionGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Partition update mutation completed for partition {partitionGuid} by actor {actorId}.")]
    public static partial void UpdateCompleted(
        this ILogger logger,
        Guid partitionGuid,
        ActorId actorId);
}
