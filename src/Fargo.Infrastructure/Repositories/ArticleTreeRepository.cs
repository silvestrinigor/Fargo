using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Persistence;

namespace Fargo.Infrastructure.Repositories;

public sealed class ArticleTreeRepository(FargoDbContext dbContext) : IArticleTreeRepository
{
    public Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        Guid? articleGuid,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<TreeNode>> GetMembersInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? articleGuid,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
