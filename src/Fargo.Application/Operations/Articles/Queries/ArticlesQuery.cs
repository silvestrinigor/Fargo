using Fargo.Application.Abstractions;
using Fargo.Application.Common;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Paging;
using Fargo.Application.Operations.Articles.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.Articles;

/// <param name="Sort">For example <c>name</c> or <c>-name</c>.</param>
/// <param name="Search">Case-insensitive match against the name or description.</param>
public sealed record ArticlesQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<ArticleDto>>;

public sealed class ArticlesQueryHandler(
    ActorResolver actorService,
    IArticleQueryRepository articleRepository,
    ICurrentActor currentActor
) : IQueryHandler<ArticlesQuery, PagedResult<ArticleDto>>
{
    public async Task<PagedResult<ArticleDto>> HandleAsync(ArticlesQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var partitionGuids =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessGuids,
                query.ChildOfAnyOfThesePartitions
            );

        var articles = await articleRepository.GetManyInfoOrderedByGuidAsync(
            query.WithPagination,
            partitionGuids,
            query.Sort,
            query.Search,
            cancellationToken
        );

        return articles;
    }
}
