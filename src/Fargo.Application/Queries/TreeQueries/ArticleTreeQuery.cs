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
    : IQuery<IReadOnlyCollection<TreeNode>>;

public sealed class ArticleTreeQueryHandler(
        ActorService actorService,
    IArticleTreeRepository articleTreeRepository,
    ICurrentUser currentUser)
    : IQueryHandler<ArticleTreeQuery, IReadOnlyCollection<TreeNode>>
{
    public async Task<IReadOnlyCollection<TreeNode>> Handle(
        ArticleTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedUserActorByGuid(currentUser.UserGuid, cancellationToken);

        return await articleTreeRepository.GetMembers(
            query.Pagination ?? Pagination.FirstPage20Items,
            actor.PartitionAccesses,
            query.ArticleGuid,
            cancellationToken);
    }
}
