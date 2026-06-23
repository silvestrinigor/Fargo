using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticlesQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Articles query started for actor {actorId}. Page: {Page}. Limit: {Limit}.")]
    public static partial void ArticlesQueryStarted(
        this ILogger logger,
        ActorId articleId,
        Page page,
        Limit limit);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Articles query completed for actor {actorId}. RequestedPartitionCount: {requestedPartitionCount}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.")]
    public static partial void ArticlesQueryCompleted(
        this ILogger logger,
        ActorId actorId,
        int requestedPartitionCount,
        int effectivePartitionCount,
        int resultCount);
}
