using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.TreeQueries;

public sealed record UserGroupTreeQuery(
    Guid? UserGroupGuid = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<TreeNode>>;

public sealed class UserGroupTreeQueryHandler(
    IUserGroupTreeRepository userGroupTreeRepository,
    IPartitionRepository partitionRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser)
    : IQueryHandler<UserGroupTreeQuery, IReadOnlyCollection<TreeNode>>
{
    public async Task<IReadOnlyCollection<TreeNode>> Handle(
        UserGroupTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        var accessiblePartitionGuids = await partitionRepository.GetDescendantGuids(
            [.. actor.PartitionAccesses.Select(x => x.PartitionGuid)],
            includeRoots: true,
            cancellationToken);

        return await userGroupTreeRepository.GetMembers(
            query.Pagination ?? Pagination.FirstPage20Items,
            accessiblePartitionGuids,
            query.UserGroupGuid,
            cancellationToken);
    }
}
