using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.TreeQueries;

public sealed record PartitionSecurityTreeQuery(
    Guid? PartitionGuid = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<TreeNode>>;

public sealed class PartitionSecurityTreeQueryHandler(
    IPartitionSecurityTreeRepository partitionSecurityTreeRepository,
    IPartitionRepository partitionRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser)
    : IQueryHandler<PartitionSecurityTreeQuery, IReadOnlyCollection<TreeNode>>
{
    public async Task<IReadOnlyCollection<TreeNode>> Handle(
        PartitionSecurityTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        var accessiblePartitionGuids = await partitionRepository.GetDescendantGuids(
            [.. actor.PartitionAccesses.Select(x => x.PartitionGuid)],
            includeRoots: true,
            cancellationToken);

        return await partitionSecurityTreeRepository.GetMembers(
            query.Pagination ?? Pagination.FirstPage20Items,
            accessiblePartitionGuids,
            query.PartitionGuid,
            cancellationToken);
    }
}
