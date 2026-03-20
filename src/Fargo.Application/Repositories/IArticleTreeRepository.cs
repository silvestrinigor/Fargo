using Fargo.Application.Models.TreeModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Repositories;

public interface IArticleTreeRepository
{
    Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? articleGuid,
        CancellationToken cancellationToken = default);
}
