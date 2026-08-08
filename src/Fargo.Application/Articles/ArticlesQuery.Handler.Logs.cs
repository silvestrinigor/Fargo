using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticlesQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Articles query started for actor {actorGuid}. Page: {Page}. Limit: {Limit}.")]
    public static partial void ArticlesQueryStarted(
        this ILogger logger,
        Guid actorGuid,
        Page page,
        Limit limit);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Articles query completed for actor {actorGuid}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.")]
    public static partial void ArticlesQueryCompleted(
        this ILogger logger,
        Guid actorGuid,
        int requestedPartitionCount,
        int effectivePartitionCount,
        int resultCount);
}
