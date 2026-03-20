using Fargo.Application.Models.TreeModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Repositories;

public interface IPartitionSecurityTreeRepository
{
    Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? partitionGuid,
        CancellationToken cancellationToken = default);
}
