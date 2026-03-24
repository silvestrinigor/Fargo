using Fargo.Application.Models.TreeModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Repositories;

public interface IPartitionTreeRepository
{
    Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        Guid? parentPartitionGuid,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TreeNode>> GetMembersFilteredByAccess(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? parentPartitionGuid,
        CancellationToken cancellationToken = default);
}
