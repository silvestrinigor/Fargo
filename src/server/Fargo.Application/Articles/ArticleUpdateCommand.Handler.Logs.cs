using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleUpdateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article update flow started for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void ArticleUpdateStarted(
        this ILogger logger,
        Guid articleGuid,
        Guid actorGuid,
        ActorType actorType
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article update flow completed for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void ArticleUpdateCompleted(
        this ILogger logger,
        Guid articleGuid,
        Guid actorGuid,
        ActorType actorType
    );
}
