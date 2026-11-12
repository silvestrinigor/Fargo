using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Dtos.Articles;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;
namespace Fargo.Application.Queries.Articles;

public sealed class ArticleQueryHandler(
    ActorResolver actorService, IArticleQueryRepository articleRepository,
    ICurrentActor currentActor
) : IQueryHandler<ArticleByGuidQuery, ArticleDto?>,
    IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>
{
    public async Task<ArticleDto?> HandleAsync(
        ArticleByGuidQuery query, CancellationToken cancellationToken = default)
    {
        var article = await QueryAsync(
            (repository, partitions, ct) => repository.GetInfoByGuidAsync(query.ArticleGuid, partitions, ct),
            cancellationToken);

        return article;
    }

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

