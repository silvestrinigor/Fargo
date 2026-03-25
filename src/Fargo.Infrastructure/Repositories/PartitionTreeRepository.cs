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

    private readonly DbSet<Item> items = dbContext.Items;

    private readonly DbSet<Article> articles = dbContext.Articles;

    private readonly DbSet<User> users = dbContext.Users;

    private readonly DbSet<UserGroup> userGroups = dbContext.UserGroups;

    public async Task<IReadOnlyCollection<EntityTreeNode>> GetPartitionTreeNodes(
            Pagination pagination,
            Guid? partitionGuid,
            IReadOnlyCollection<TreeNodeType>? includedTypes = null,
            CancellationToken cancellationToken = default)
    {
        includedTypes ??= Enum.GetValues<TreeNodeType>();

        IQueryable<EntityTreeNode> query = Enumerable.Empty<EntityTreeNode>().AsQueryable();

        if (includedTypes.Contains(TreeNodeType.Partition))
        {
            var partitionQuery = partitions
                .Where(p => p.ParentPartitionGuid == partitionGuid)
                .Select(p => new EntityTreeNode(
                    new Nodeid(TreeNodeType.Partition, p.Guid),
                    p.Name,
                    p.Description,
                    p.PartitionMembers.Any(),
                    p.IsActive
                ));

            query = query.Concat(partitionQuery);
        }

        if (includedTypes.Contains(TreeNodeType.User))
        {
            var usersQuery = users
                .Where(u =>
                    partitionGuid == null
                        ? !u.Partitions.Any()
                        : u.Partitions.Any(p => p.Guid == partitionGuid))
                .Select(u => new EntityTreeNode(
                    new Nodeid(TreeNodeType.User, u.Guid),
                    u.Nameid,
                    u.Description,
                    false,
                    u.IsActive
                ));

            query = query.Concat(usersQuery);
        }

        if (includedTypes.Contains(TreeNodeType.UserGroup))
        {
            var groupsQuery = userGroups
                .Where(g =>
                    partitionGuid == null
                        ? !g.Partitions.Any()
                        : g.Partitions.Any(p => p.Guid == partitionGuid))
                .Select(g => new EntityTreeNode(
                    new Nodeid(TreeNodeType.UserGroup, g.Guid),
                    g.Nameid,
                    g.Description,
                    false,
                    g.IsActive
                ));

            query = query.Concat(groupsQuery);
        }

        if (includedTypes.Contains(TreeNodeType.Article))
        {
            var articlesQuery = articles
                .Where(a =>
                    partitionGuid == null
                        ? !a.Partitions.Any()
                        : a.Partitions.Any(p => p.Guid == partitionGuid))
                .Select(a => new EntityTreeNode(
                    new Nodeid(TreeNodeType.Article, a.Guid),
                    a.Name,
                    a.Description,
                    false,
                    true
                    ));

            query = query.Concat(articlesQuery);
        }

        if (includedTypes.Contains(TreeNodeType.Item))
        {
            var itemsQuery = items
                .Where(i =>
                    partitionGuid == null
                        ? !i.Partitions.Any()
                        : i.Partitions.Any(p => p.Guid == partitionGuid))
                .Select(i => new EntityTreeNode(
                    new Nodeid(TreeNodeType.Item, i.Guid),
                    i.Article.Name,
                    i.Article.Description,
                    false,
                    true
                ));

            query = query.Concat(itemsQuery);
        }

        return await query
            .OrderBy(x => x.Title)
            .ThenBy(x => x.TreeNodeType)
            .ThenBy(x => x.EntityGuid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EntityTreeNode>> GetPartitionTreeNodesInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? partitionGuid,
        IReadOnlyCollection<TreeNodeType>? includedTypes = null,
        CancellationToken cancellationToken = default)
    {
        includedTypes ??= Enum.GetValues<TreeNodeType>();

        IQueryable<EntityTreeNode>? query = null;

        if (includedTypes.Contains(TreeNodeType.Partition))
        {
            var partitionQuery = partitions
                .Where(p =>
                    p.ParentPartitionGuid == partitionGuid &&
                    partitionGuids.Contains(p.Guid))
                .Select(p => new EntityTreeNode(
                    new Nodeid(TreeNodeType.Partition, p.Guid),
                    p.Name,
                    p.Description,
                    p.PartitionMembers.Any(),
                    p.IsActive
                    ));

            query = query == null ? partitionQuery : query.Concat(partitionQuery);
        }

        if (includedTypes.Contains(TreeNodeType.User))
        {
            var usersQuery = users
                .Where(u =>
                    partitionGuid == null
                    ? !u.Partitions.Any() ||
                    u.Partitions.Any(p => partitionGuids.Contains(p.Guid))
                    : u.Partitions.Any(p =>
                        p.Guid == partitionGuid &&
                        partitionGuids.Contains(p.Guid)))
                .Select(u => new EntityTreeNode(
                    new Nodeid(TreeNodeType.User, u.Guid),
                    u.Nameid,
                    u.Description,
                    false,
                    u.IsActive
                    ));

            query = query == null ? usersQuery : query.Concat(usersQuery);
        }

        if (includedTypes.Contains(TreeNodeType.UserGroup))
        {
            var groupsQuery = userGroups
                .Where(g =>
                    partitionGuid == null
                    ? !g.Partitions.Any() ||
                    g.Partitions.Any(p => partitionGuids.Contains(p.Guid))
                    : g.Partitions.Any(p =>
                        p.Guid == partitionGuid &&
                        partitionGuids.Contains(p.Guid)))
                .Select(g => new EntityTreeNode(
                    new Nodeid(TreeNodeType.UserGroup, g.Guid),
                    g.Nameid,
                    g.Description,
                    false,
                    g.IsActive
                    ));

            query = query == null ? groupsQuery : query.Concat(groupsQuery);
        }

        if (includedTypes.Contains(TreeNodeType.Article))
        {
            var articlesQuery = articles
                .Where(a =>
                    partitionGuid == null
                    ? !a.Partitions.Any() ||
                    a.Partitions.Any(p => partitionGuids.Contains(p.Guid))
                    : a.Partitions.Any(p =>
                        p.Guid == partitionGuid &&
                        partitionGuids.Contains(p.Guid)))
                .Select(a => new EntityTreeNode(
                    new Nodeid(TreeNodeType.Article, a.Guid),
                    a.Name,
                    a.Description,
                    false,
                    true
                    ));

            query = query == null ? articlesQuery : query.Concat(articlesQuery);
        }

        if (includedTypes.Contains(TreeNodeType.Item))
        {
            var itemsQuery = items
                .Where(i =>
                    partitionGuid == null
                    ? !i.Partitions.Any() ||
                    i.Partitions.Any(p => partitionGuids.Contains(p.Guid))
                    : i.Partitions.Any(p =>
                        p.Guid == partitionGuid &&
                        partitionGuids.Contains(p.Guid)))
                .Select(i => new EntityTreeNode(
                    new Nodeid(TreeNodeType.Item, i.Guid),
                    i.Article.Name,
                    i.Article.Description,
                    false,
                    true
                    ));

            query = query == null ? itemsQuery : query.Concat(itemsQuery);
        }

        query ??= Enumerable.Empty<EntityTreeNode>().AsQueryable();

        return await query
            .OrderBy(x => x.Title)
            .ThenBy(x => x.TreeNodeType)
            .ThenBy(x => x.EntityGuid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);
    }
}
