using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article create flow started for actor '{actorId}'.")]
    public static partial void CreateStarted(
        this ILogger logger,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article create mutation completed for article '{articleGuid}' by actor '{actorId}'.")]
    public static partial void CreateCompleted(
        this ILogger logger,
        Guid articleGuid,
        ActorId actorId);
}
