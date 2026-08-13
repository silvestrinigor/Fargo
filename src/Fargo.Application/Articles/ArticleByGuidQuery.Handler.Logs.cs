using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleByGuidQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Article query by guid started for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void QueryByGuidStarted(
        this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Article single query completed for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'. Found: {found}.")]
    public static partial void QueryByGuidCompleted(
        this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType, bool found);
}
