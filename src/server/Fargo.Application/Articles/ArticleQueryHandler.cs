using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles the single-article lookup queries (<see cref="ArticleByGuidQuery"/>
/// and <see cref="ArticleByBarcodeQuery"/>). Both lookups share the same actor
/// resolution, partition-scoping, and result shape; only the repository lookup
/// differs, so they are served by one handler.
/// </summary>
/// <param name="actorService">Resolves the current actor and its partition access.</param>
/// <param name="articleRepository">Provides access to article query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
public sealed class ArticleQueryHandler(
    ActorResolver actorService, IArticleQueryRepository articleRepository,
    ICurrentActor currentActor
) : IQueryHandler<ArticleByGuidQuery, ArticleDto?>,
    IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>
{
    /// <summary>
    /// Retrieves an article by its unique identifier within the current actor's
    /// accessible partitions.
    /// </summary>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the current actor cannot be found.
    /// </exception>
    public async Task<ArticleDto?> HandleAsync(
        ArticleByGuidQuery query, CancellationToken cancellationToken = default)
    {
        var article = await QueryAsync(
            (repository, partitions, ct) => repository.GetInfoByGuidAsync(query.ArticleGuid, partitions, ct),
            cancellationToken);

        return article;
    }

    /// <summary>
    /// Retrieves an article by its barcode within the current actor's accessible
    /// partitions.
    /// </summary>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the current actor cannot be found.
    /// </exception>
    public async Task<ArticleDto?> HandleAsync(
        ArticleByBarcodeQuery query, CancellationToken cancellationToken = default)
    {
        var article = await QueryAsync(
            (repository, partitions, ct) => repository.GetInfoByBarcodeAsync(query.ArticleBarcode, partitions, ct),
            cancellationToken);

        return article;
    }

    private async Task<ArticleDto?> QueryAsync(
        Func<IArticleQueryRepository, IReadOnlyCollection<Guid>, CancellationToken, Task<ArticleDto?>> lookup,
        CancellationToken cancellationToken)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        return await lookup(articleRepository, actor.PartitionAccessGuids, cancellationToken);
    }
}
