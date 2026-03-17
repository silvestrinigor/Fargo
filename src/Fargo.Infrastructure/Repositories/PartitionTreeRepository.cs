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

    public async Task<IReadOnlyCollection<TreeNode>> GetMembers(Pagination pagination, Guid? parentPartitionGuid, CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.Partitions
            .AsNoTracking()
            .AsQueryable();

        baseQuery = parentPartitionGuid == null
            ? baseQuery.Where(x => x.ParentPartitionGuid == null)
            : baseQuery.Where(x => x.ParentPartitionGuid == parentPartitionGuid);

        var rows = await baseQuery
            .OrderBy(x => x.Name.Value)
            .WithPagination(pagination)
            .Select(x => new
            {
                x.Guid,
                Name = x.Name.Value,
                Description = x.Description.Value,
                x.ParentPartitionGuid,
                x.IsActive,
                HasPartitionMembers = partitions.Any(c => c.ParentPartitionGuid == x.Guid),
                PartitionMembersCount = partitions.Count(c => c.ParentPartitionGuid == x.Guid)
            })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(x =>
            new TreeNode(
                Nodeid: new Nodeid(TreeNodeType.Partition, x.Guid),
                Title: x.Name,
                Subtitle: x.Description,
                ParentNodeId: x.ParentPartitionGuid is null
                    ? null
                    : new Nodeid(TreeNodeType.Partition, x.ParentPartitionGuid.Value),
                MembersCount: x.PartitionMembersCount,
                IsActive: x.IsActive
                )
            )];
    }
}
