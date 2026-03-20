using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class PartitionSecurityTreeRepository(FargoDbContext dbContext) : IPartitionSecurityTreeRepository
{
    private readonly DbSet<Partition> partitions = dbContext.Partitions;
    private readonly DbSet<UserGroup> userGroups = dbContext.UserGroups;
    private readonly DbSet<User> users = dbContext.Users;

    public async Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? partitionGuid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessiblePartitionGuids);

        if (accessiblePartitionGuids.Count == 0)
        {
            return [];
        }

        var visiblePartitionGuids = accessiblePartitionGuids.Distinct().ToArray();

        if (partitionGuid == null)
        {
            var rows = await partitions
                .AsNoTracking()
                .Where(partition => visiblePartitionGuids.Contains(partition.Guid))
                .Where(partition => partition.ParentPartitionGuid == null)
                .OrderBy(partition => partition.Name)
                .WithPagination(pagination)
                .Select(partition => new
                {
                    partition.Guid,
                    Title = partition.Name.Value,
                    Subtitle = partition.Description.Value,
                    partition.IsActive,
                    MembersCount =
                        userGroups.Count(group =>
                            group.PartitionAccesses.Any(access => access.PartitionGuid == partition.Guid) &&
                            group.Partitions.Any(entityPartition => visiblePartitionGuids.Contains(entityPartition.Guid)))
                        +
                        users.Count(user =>
                            user.PartitionAccesses.Any(access => access.PartitionGuid == partition.Guid) &&
                            user.Partitions.Any(entityPartition => visiblePartitionGuids.Contains(entityPartition.Guid)))
                })
                .ToListAsync(cancellationToken);

            return
            [
                .. rows.Select(x => new TreeNode(
                    Nodeid: TreeNodeIdFactory.Create(TreeNodeType.Partition, x.Guid),
                    TreeNodeType: TreeNodeType.Partition,
                    EntityGuid: x.Guid,
                    Title: x.Title,
                    Subtitle: string.IsNullOrWhiteSpace(x.Subtitle) ? null : x.Subtitle,
                    ParentNodeId: null,
                    MembersCount: x.MembersCount,
                    IsActive: x.IsActive))
            ];
        }

        var parentNodeId = TreeNodeIdFactory.Create(TreeNodeType.Partition, partitionGuid.Value);

        var groupRows = await userGroups
            .AsNoTracking()
            .Where(group => group.PartitionAccesses.Any(access => access.PartitionGuid == partitionGuid.Value))
            .Where(group => group.Partitions.Any(entityPartition => visiblePartitionGuids.Contains(entityPartition.Guid)))
            .OrderBy(group => group.Nameid.Value)
            .WithPagination(pagination)
            .Select(group => new
            {
                group.Guid,
                Title = group.Nameid.Value,
                Subtitle = group.Description.Value,
                group.IsActive
            })
            .ToListAsync(cancellationToken);

        var userRows = await users
            .AsNoTracking()
            .Where(user => user.PartitionAccesses.Any(access => access.PartitionGuid == partitionGuid.Value))
            .Where(user => user.Partitions.Any(entityPartition => visiblePartitionGuids.Contains(entityPartition.Guid)))
            .OrderBy(user => user.Nameid.Value)
            .WithPagination(pagination)
            .Select(user => new
            {
                user.Guid,
                Title = user.Nameid.Value,
                Subtitle = ((user.FirstName != null ? user.FirstName.Value : string.Empty) + " " +
                            (user.LastName != null ? user.LastName.Value : string.Empty)).Trim(),
                user.IsActive
            })
            .ToListAsync(cancellationToken);

        var nodes = new List<TreeNode>(groupRows.Count + userRows.Count);

        nodes.AddRange(groupRows.Select(x => new TreeNode(
            Nodeid: TreeNodeIdFactory.Create(TreeNodeType.UserGroup, x.Guid, parentNodeId),
            TreeNodeType: TreeNodeType.UserGroup,
            EntityGuid: x.Guid,
            Title: x.Title,
            Subtitle: string.IsNullOrWhiteSpace(x.Subtitle) ? null : x.Subtitle,
            ParentNodeId: parentNodeId,
            MembersCount: 0,
            IsActive: x.IsActive)));

        nodes.AddRange(userRows.Select(x => new TreeNode(
            Nodeid: TreeNodeIdFactory.Create(TreeNodeType.User, x.Guid, parentNodeId),
            TreeNodeType: TreeNodeType.User,
            EntityGuid: x.Guid,
            Title: x.Title,
            Subtitle: string.IsNullOrWhiteSpace(x.Subtitle) ? null : x.Subtitle,
            ParentNodeId: parentNodeId,
            MembersCount: 0,
            IsActive: x.IsActive)));

        return nodes;
    }
}
