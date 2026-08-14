using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article create flow started for actor '{actorGuid}' of the type {actorType}.")]
    public static partial void CreateStarted(
        this ILogger logger,
        Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article create mutation completed for article '{articleGuid}' by actor '{actorGuid}' of the type {actorType}.")]
    public static partial void CreateCompleted(
        this ILogger logger,
        Guid articleGuid, Guid actorGuid, ActorType actorType);
}
