using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.TreeQueries;

public sealed record ArticleTreeQuery(
        Guid? ArticleGuid = null,
        Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<EntityTreeNode>>;

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
