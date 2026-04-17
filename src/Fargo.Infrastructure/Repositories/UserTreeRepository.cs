using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class UserTreeRepository(FargoDbContext dbContext) : IUserTreeRepository
{
    private readonly DbSet<UserGroup> userGroups = dbContext.UserGroups;
    private readonly DbSet<User> users = dbContext.Users;

    public async Task<IReadOnlyCollection<EntityTreeNode>> GetUserTreeNodes(
        Pagination pagination,
        Guid? userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        var userTreeQuery = users
            .Where(u =>
                (userGroupGuid == null && !u.UserGroups.Any()) ||
                (userGroupGuid != null && u.UserGroups.Any(g => g.Guid == userGroupGuid)))
            .Select(u => new EntityTreeNode(
                new Nodeid(TreeNodeType.Partition, u.Guid),
                u.Nameid,
                u.Description,
                HasChildren: false,
                u.IsActive
            ));

        var userGroupTreeQuery = userGroups
            .Select(userGroup => new EntityTreeNode(
                new Nodeid(TreeNodeType.Partition, userGroup.Guid),
                userGroup.Nameid,
                userGroup.Description,
                HasChildren: users.Any(u => u.UserGroups.Any(g => g.Guid == userGroup.Guid)),
                userGroup.IsActive
            ));

        var userAndUserGroupTreeQuery = userGroupGuid is null ? userTreeQuery.Concat(userGroupTreeQuery) : userTreeQuery;

        return await userAndUserGroupTreeQuery
            .WithPagination(pagination)
            .OrderBy(t => t.Title)
            .ThenBy(t => t.EntityGuid)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EntityTreeNode>> GetUserTreeNodesInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        var userTreeQuery = users
            .Where(u =>
                (userGroupGuid == null && !u.UserGroups.Any()) ||
                (userGroupGuid != null && u.UserGroups.Any(g => g.Guid == userGroupGuid)))
            .Where(u => u.Partitions.Any(p => accessiblePartitionGuids.Contains(p.Guid)))
            .Select(u => new EntityTreeNode(
                new Nodeid(TreeNodeType.Partition, u.Guid),
                u.Nameid,
                u.Description,
                HasChildren: false,
                u.IsActive
            ));

        var userGroupTreeQuery = userGroups
            .Where(u => u.Partitions.Any(p => accessiblePartitionGuids.Contains(p.Guid)))
            .Select(userGroup => new EntityTreeNode(
                new Nodeid(TreeNodeType.Partition, userGroup.Guid),
                userGroup.Nameid,
                userGroup.Description,
                HasChildren: users.Any(u => u.UserGroups.Any(g => g.Guid == userGroup.Guid)),
                userGroup.IsActive
            ));

        var userAndUserGroupTreeQuery = userGroupGuid is null ? userTreeQuery.Concat(userGroupTreeQuery) : userTreeQuery;

        return await userAndUserGroupTreeQuery
            .WithPagination(pagination)
            .OrderBy(t => t.Title)
            .ThenBy(t => t.EntityGuid)
            .ToListAsync(cancellationToken);
    }
}
