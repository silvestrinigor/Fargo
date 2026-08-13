using Fargo.Application.Common;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

internal static partial class UsersQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Users query started for actor {actorGuid}. Page: {page}. Limit: {limit}.")]
    public static partial void ManyQueryStarted(
        this ILogger logger, Guid actorGuid, Page page, Limit limit);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Users query completed for actor {actorGuid}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.")]
    public static partial void ManyQueryCompleted(
        this ILogger logger, Guid actorGuid,
        int requestedPartitionCount, int effectivePartitionCount, int resultCount);
}
