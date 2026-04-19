using Fargo.Application.Tree;

using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ArticleTreeRepository(FargoDbContext dbContext) : IArticleTreeRepository
{
    private readonly DbSet<Article> articles = dbContext.Articles;

    private readonly DbSet<Item> items = dbContext.Items;

    public async Task<IReadOnlyCollection<EntityTreeNode>> GetArticleItemTreeNodesInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var articleItemTreeQuery = items
            .Where(i => i.ArticleGuid == articleGuid)
            .Where(p => p.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .Select(i => new EntityTreeNode(
                new Nodeid(TreeNodeType.Item, i.Guid),
                i.Article.Name,
                i.Article.Description,
                HasChildren: false,
                IsActive: false
            ));

        return await articleItemTreeQuery
            .WithPagination(pagination)
            .OrderBy(t => t.Title)
            .ThenBy(t => t.EntityGuid)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EntityTreeNode>> GetArticleTreeNodes(
        Pagination pagination,
        Guid? articleGuid,
        CancellationToken cancellationToken = default)
    {
        var articleItemTreeQuery = items
            .Where(i => i.ArticleGuid == articleGuid)
            .Select(i => new EntityTreeNode(
                new Nodeid(TreeNodeType.Partition, i.Guid),
                i.Article.Name,
                i.Article.Description,
                HasChildren: false,
                IsActive: false
            ));

        var articleTreeQuery = articles
            .Select(a => new EntityTreeNode(
                new Nodeid(TreeNodeType.Article, a.Guid),
                a.Name,
                a.Description,
                HasChildren: items.Any(i => i.ArticleGuid == a.Guid),
                IsActive: false
            ));

        var articleAndItemTreeQuery = articleGuid is null
            ? articleTreeQuery
            : articleItemTreeQuery;

        return await articleAndItemTreeQuery
            .WithPagination(pagination)
            .OrderBy(t => t.Title)
            .ThenBy(t => t.EntityGuid)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EntityTreeNode>> GetArticleTreeNodesInPartitions(
        Pagination pagination,
        Guid? articleGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        CancellationToken cancellationToken = default)
    {
        var articleItemTreeQuery = items
            .Where(i => i.ArticleGuid == articleGuid)
            .Where(i => i.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .Select(i => new EntityTreeNode(
                new Nodeid(TreeNodeType.Item, i.Guid),
                i.Article.Name,
                i.Article.Description,
                HasChildren: false,
                IsActive: false
            ));

        var articleTreeQuery = articles
            .Where(a => a.Partitions.Any(p => partitionGuids.Contains(a.Guid)))
            .Select(a => new EntityTreeNode(
                new Nodeid(TreeNodeType.Article, a.Guid),
                a.Name,
                a.Description,
                HasChildren: items.Any(i => i.ArticleGuid == a.Guid),
                IsActive: false
            ));

        var articleAndItemTreeQuery = articleGuid is null
            ? articleTreeQuery
            : articleItemTreeQuery;

        return await articleTreeQuery
            .WithPagination(pagination)
            .OrderBy(t => t.Title)
            .ThenBy(t => t.EntityGuid)
            .ToListAsync(cancellationToken);
    }
}
