using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.TreeQueries;

public sealed record ArticleTreeQuery(
    Guid? ArticleGuid = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<TreeNode>>;

public sealed class ArticleTreeQueryHandler(
    IArticleTreeRepository articleTreeRepository,
    IPartitionRepository partitionRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser)
    : IQueryHandler<ArticleTreeQuery, IReadOnlyCollection<TreeNode>>
{
    public async Task<IReadOnlyCollection<TreeNode>> Handle(
        ArticleTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        var accessiblePartitionGuids = await partitionRepository.GetDescendantGuids(
            [.. actor.PartitionAccesses.Select(x => x.PartitionGuid)],
            includeRoots: true,
            cancellationToken);

        return await articleTreeRepository.GetMembers(
            query.Pagination ?? Pagination.FirstPage20Items,
            accessiblePartitionGuids,
            query.ArticleGuid,
            cancellationToken);
    }
}
