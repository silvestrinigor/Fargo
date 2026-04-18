using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Tree;

/// <summary>
/// Query used to retrieve a paginated collection of article tree nodes.
/// </summary>
/// <param name="ArticleGuid">
/// The unique identifier of the article to start from.
/// If <c>null</c>, all accessible articles are returned.
/// </param>
/// <param name="Pagination">
/// The pagination settings used to limit and organize the result set.
/// If not provided, defaults to <see cref="Pagination.FirstPage20Items"/>.
/// </param>
public sealed record ArticleTreeQuery(
        Guid? ArticleGuid = null,
        Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<EntityTreeNode>>;

/// <summary>
/// Handles <see cref="ArticleTreeQuery"/> requests.
/// </summary>
/// <remarks>
/// Admins and system actors receive all article tree nodes.
/// Regular actors only receive nodes for articles located in their accessible partitions.
/// </remarks>
public sealed class ArticleTreeQueryHandler(
        ActorService actorService,
        IArticleTreeRepository articleTreeRepository,
        ICurrentUser currentUser)
    : IQueryHandler<ArticleTreeQuery, IReadOnlyCollection<EntityTreeNode>>
{
    public async Task<IReadOnlyCollection<EntityTreeNode>> Handle(
            ArticleTreeQuery query,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            return await articleTreeRepository.GetArticleTreeNodes(
                query.Pagination ?? Pagination.FirstPage20Items,
                query.ArticleGuid,
                cancellationToken
                );
        }

        return await articleTreeRepository.GetArticleTreeNodesInPartitions(
            query.Pagination ?? Pagination.FirstPage20Items,
            query.ArticleGuid,
            actor.PartitionAccesses,
            cancellationToken
            );
    }
}
