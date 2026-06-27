using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleByGuidQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article single query started for article {articleGuid} by actor {actorId}..")]
    public static partial void QueryByGuidStarted(
        this ILogger logger,
        Guid articleGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article single query completed for article {articleGuid} by actor {actorId}. Found: {found}.")]
    public static partial void QueryByGuidCompleted(
        this ILogger logger,
        Guid articleGuid,
        ActorId actorId,
        bool found);
}
