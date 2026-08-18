using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles queries that retrieve an article by its unique identifier.
/// </summary>
/// <param name="actorService">Resolves the current actor and its partition access.</param>
/// <param name="articleRepository">Provides access to article query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
/// <param name="logger">Logs the execution of the query.</param>
public sealed class ArticleByGuidQueryHandler(
    ActorResolver actorService, IArticleQueryRepository articleRepository,
    ICurrentActor currentActor, ILogger<ArticleByGuidQueryHandler> logger
) : IQueryHandler<ArticleByGuidQuery, ArticleDto?>
{
    /// <summary>
    /// Retrieves an article matching the requested identifier within the current
    /// actor's accessible partitions.
    /// </summary>
    /// <param name="query">The query containing the article identifier to search for.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// The matching article, or <see langword="null"/> if no accessible article
    /// matches the specified identifier.
    /// </returns>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the current actor cannot be found.
    /// </exception>
    public async Task<ArticleDto?> HandleAsync(
        ArticleByGuidQuery query, CancellationToken cancellationToken = default)
    {
        logger.QueryByGuidStarted(query.ArticleGuid, currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var article = await articleRepository.GetInfoByGuidAsync(
            query.ArticleGuid,
            childOfAnyOfThesePartitions: actor.PartitionAccessGuids,
            cancellationToken);

        logger.QueryByGuidCompleted(query.ArticleGuid, currentActor.Guid, currentActor.ActorType, found: article is not null);

        return article;
    }
}
