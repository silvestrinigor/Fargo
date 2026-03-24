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

    public Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        Guid? parentPartitionGuid,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<TreeNode>> GetMembersFilteredByAccess(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? parentPartitionGuid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessiblePartitionGuids);

        if (accessiblePartitionGuids.Count == 0)
        {
            return [];
        }

        var visiblePartitionGuids = accessiblePartitionGuids.Distinct().ToArray();

        var baseQuery = partitions
            .AsNoTracking()
            .Where(x => visiblePartitionGuids.Contains(x.Guid));

        baseQuery = parentPartitionGuid == null
            ? baseQuery.Where(x => x.ParentPartitionGuid == null)
            : baseQuery.Where(x => x.ParentPartitionGuid == parentPartitionGuid);

        var rows = await baseQuery
            .OrderBy(x => x.Name)
            .WithPagination(pagination)
            .Select(x => new
            {
                x.Guid,
                Title = x.Name.Value,
                Subtitle = x.Description.Value,
                x.ParentPartitionGuid,
                x.IsActive,
                MembersCount = partitions.Count(c =>
                    c.ParentPartitionGuid == x.Guid &&
                    visiblePartitionGuids.Contains(c.Guid))
            })
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(x => new TreeNode(
                Nodeid: TreeNodeIdFactory.Create(TreeNodeType.Partition, x.Guid),
                Title: x.Title,
                Subtitle: string.IsNullOrWhiteSpace(x.Subtitle) ? null : x.Subtitle,
                ParentNodeId: x.ParentPartitionGuid is null
                    ? null
                    : TreeNodeIdFactory.Create(TreeNodeType.Partition, x.ParentPartitionGuid.Value),
                MembersCount: x.MembersCount,
                IsActive: x.IsActive))
        ];
    }
}
