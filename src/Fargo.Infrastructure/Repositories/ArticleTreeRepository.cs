using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ArticleTreeRepository(FargoDbContext dbContext) : IArticleTreeRepository
{
    private readonly DbSet<Article> articles = dbContext.Articles;
    private readonly DbSet<Item> items = dbContext.Items;

    public Task<IReadOnlyCollection<TreeNode>> GetMembers(
        Pagination pagination,
        Guid? articleGuid,
        CancellationToken cancellationToken = default)
    {
        if (articleGuid is not null)
        {

        }
    }

    public async Task<IReadOnlyCollection<TreeNode>> GetMembersInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? articleGuid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessiblePartitionGuids);

        if (accessiblePartitionGuids.Count == 0)
        {
            return [];
        }

        var visiblePartitionGuids = accessiblePartitionGuids.Distinct().ToArray();

        if (articleGuid == null)
        {
            var rows = await articles
                .AsNoTracking()
                .Where(article => article.Partitions.Any(partition => visiblePartitionGuids.Contains(partition.Guid)))
                .OrderBy(article => article.Name)
                .WithPagination(pagination)
                .Select(article => new
                {
                    article.Guid,
                    Title = article.Name.Value,
                    Subtitle = article.Description.Value,
                    MembersCount = items.Count(item =>
                        item.ArticleGuid == article.Guid &&
                        item.Partitions.Any(partition => visiblePartitionGuids.Contains(partition.Guid)))
                })
                .ToListAsync(cancellationToken);

            return
                [
                .. rows.Select(x => new TreeNode(
                    Nodeid: TreeNodeIdFactory.Create(TreeNodeType.Article, x.Guid),
                    Title: x.Title,
                    Subtitle: string.IsNullOrWhiteSpace(x.Subtitle) ? null : x.Subtitle,
                    ParentNodeId: null,
                    MembersCount: x.MembersCount
                    ))
                ];
        }

        var parentNodeId = TreeNodeIdFactory.Create(TreeNodeType.Article, articleGuid.Value);

        var itemRows = await items
            .AsNoTracking()
            .Where(item => item.ArticleGuid == articleGuid.Value)
            .Where(item => item.Partitions.Any(partition => visiblePartitionGuids.Contains(partition.Guid)))
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(item => new
            {
                item.Guid
            })
            .ToListAsync(cancellationToken);

        return
            [
            .. itemRows.Select(x => new TreeNode(
                Nodeid: TreeNodeIdFactory.Create(TreeNodeType.Item, x.Guid, parentNodeId),
                Title: x.Guid.ToString(),
                Subtitle: null,
                ParentNodeId: parentNodeId,
                MembersCount: 0,
                IsActive: true))
            ];
    }
}
