using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles.Queries;

/// <summary>
/// Query used to retrieve an article by identifier.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="AsOfDateTime">
/// Temporal query date.
/// </param>
public sealed record ArticleByGuidQuery(
    Guid ArticleGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

/// <summary>
/// Handles article queries by identifier.
/// </summary>
/// <remarks>
/// Retrieves an article visible to the current actor.
/// </remarks>
public sealed class ArticleByGuidQueryHandler(
    IArticleQueryRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleByGuidQueryHandler> logger
) : IQueryHandler<ArticleByGuidQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(
        ArticleByGuidQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article single query started for article {ArticleGuid} by actor {ActorGuid}.",
                query.ArticleGuid,
                actor.ActorGuid);
        }

        var article = await articleRepository.GetInfoByGuid(
            query.ArticleGuid,
            query.AsOfDateTime,
            childOfAnyOfThesePartitions: actor.PartitionAccesses,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article single query completed for article {ArticleGuid} by actor {ActorGuid}. Found: {Found}.",
                query.ArticleGuid,
                actor.ActorGuid,
                article is not null);
        }

        return article;
    }
}
