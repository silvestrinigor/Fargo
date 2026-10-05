using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleDeleteCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article delete flow started for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void ArticleDeleteStarted(this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article delete mutation completed for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void ArticleDeleteCompleted(this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType);
}
