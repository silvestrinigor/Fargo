using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article create flow started for actor '{actorGuid}'.")]
    public static partial void CreateStarted(
        this ILogger logger,
        Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article create mutation completed for article '{articleGuid}' by actor '{actorGuid}'.")]
    public static partial void CreateCompleted(
        this ILogger logger,
        Guid articleGuid,
        Guid actorGuid);
}
