using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleUpdateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article update flow started for article {articleGuid} by actor {actorGuid}.")]
    public static partial void UpdateStarted(
        this ILogger logger,
        Guid articleGuid,
        Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article update flow completed for article {articleGuid} by actor {actorGuid}.")]
    public static partial void UpdateCompleted(
        this ILogger logger,
        Guid articleGuid,
        Guid actorGuid);
}
