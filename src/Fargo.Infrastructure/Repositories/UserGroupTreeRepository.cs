using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserGroupTreeRepository(FargoDbContext dbContext) : IUserGroupTreeRepository
{
    private readonly DbSet<UserGroup> userGroups = dbContext.UserGroups;
    private readonly DbSet<User> users = dbContext.Users;

    public async Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessiblePartitionGuids);

        if (accessiblePartitionGuids.Count == 0)
        {
            return [];
        }

        var visiblePartitionGuids = accessiblePartitionGuids.Distinct().ToArray();

        if (userGroupGuid == null)
        {
            var r = await userGroups
                .AsNoTracking()
                .Where(userGroup => userGroup.Partitions.Any(partition => visiblePartitionGuids.Contains(partition.Guid)))
                .OrderBy(userGroup => userGroup.Nameid)
                .WithPagination(pagination)
                .Select(userGroup => new
                {
                    userGroup.Guid,
                    Title = userGroup.Nameid.Value,
                    Subtitle = userGroup.Description.Value,
                    userGroup.IsActive,
                    MembersCount = users.Count(user =>
                        user.UserGroups.Any(group => group.Guid == userGroup.Guid) &&
                        user.Partitions.Any(partition => visiblePartitionGuids.Contains(partition.Guid)))
                })
                .ToListAsync(cancellationToken);

            return
            [
                .. r.Select(x => new TreeNode(
                    Nodeid: TreeNodeIdFactory.Create(TreeNodeType.UserGroup, x.Guid),
                    TreeNodeType: TreeNodeType.UserGroup,
                    EntityGuid: x.Guid,
                    Title: x.Title,
                    Subtitle: string.IsNullOrWhiteSpace(x.Subtitle) ? null : x.Subtitle,
                    ParentNodeId: null,
                    MembersCount: x.MembersCount,
                    IsActive: x.IsActive))
            ];
        }

        var parentNodeId = TreeNodeIdFactory.Create(TreeNodeType.UserGroup, userGroupGuid.Value);

        var rows = await users
            .AsNoTracking()
            .Where(user => user.UserGroups.Any(group => group.Guid == userGroupGuid.Value))
            .Where(user => user.Partitions.Any(partition => visiblePartitionGuids.Contains(partition.Guid)))
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

        return
        [
            .. rows.Select(x => new TreeNode(
                Nodeid: TreeNodeIdFactory.Create(TreeNodeType.User, x.Guid, parentNodeId),
                TreeNodeType: TreeNodeType.User,
                EntityGuid: x.Guid,
                Title: x.Title,
                Subtitle: string.IsNullOrWhiteSpace(x.Subtitle) ? null : x.Subtitle,
                ParentNodeId: parentNodeId,
                MembersCount: 0,
                IsActive: x.IsActive))
        ];
    }
}
