using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class PartitionTreeRepository(FargoDbContext dbContext) : IPartitionTreeRepository
{
    private readonly DbSet<Partition> partitions = dbContext.Partitions;

    public async Task<IReadOnlyCollection<TreeNode>> GetPartitionChilds(
        Pagination pagination,
        Guid? partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var partitionTreeQuery = partitions
            .Where(p => p.ParentPartitionGuid == partitionGuid)
            .Select(p => new TreeNode(
                new Nodeid(TreeNodeType.Partition, p.Guid),
                p.Name,
                p.Description,
                p.PartitionMembers.Count > 0,
                p.IsActive
            ));

        return await partitionTreeQuery
            .WithPagination(pagination)
            .OrderBy(t => t.Title)
            .ThenBy(t => t.EntityGuid)
            .ToListAsync(cancellationToken);
    }
}
