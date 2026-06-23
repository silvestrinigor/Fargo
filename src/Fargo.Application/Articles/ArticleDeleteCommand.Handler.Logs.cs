using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleDeleteCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article delete flow started for article {ArticleGuid} by actor {ActorId}.")]
    public static partial void DeleteStarted(
        this ILogger logger,
        Guid articleGuid,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article delete mutation completed for article {ArticleGuid} by actor {ActorId}.")]
    public static partial void DeleteCompleted(
        this ILogger logger,
        Guid articleGuid,
        ActorId actorId);
}
