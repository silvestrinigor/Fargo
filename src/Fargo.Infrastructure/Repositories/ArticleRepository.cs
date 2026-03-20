using Fargo.Application.Mappings;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public class ArticleRepository(FargoDbContext context) : IArticleRepository
{
    private readonly DbSet<Article> articles = context.Articles;

    private readonly DbSet<Item> items = context.Items;

    public void Add(Article article) => articles.Add(article);

    public void Remove(Article article) => articles.Remove(article);

    public async Task<bool> HasItemsAssociated(Guid articleGuid, CancellationToken cancellationToken = default)
        => await items.Where(x => x.Article.Guid == articleGuid).AnyAsync(cancellationToken);

    public async Task<Article?> GetByGuid(Guid entityGuid, CancellationToken cancellationToken = default)
        => await articles
            .Include(a => a.Partitions)
            .Where(a => a.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ArticleInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(article => article.Guid == entityGuid)
            .Select(ArticleMappings.InformationProjection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ArticleInformation>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .Select(ArticleMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetManyGuids(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .Select(article => article.Guid)
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticleInformation?> GetInfoByGuidInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return null;
        }

        var query = articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        return await query
            .Where(article => article.Guid == entityGuid)
            .Where(article => article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .Select(ArticleMappings.InformationProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ArticleInformation>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        var query = articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        return await query
            .Where(article => article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .Select(ArticleMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetManyGuidsInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        var query = articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        return await query
            .Where(article => article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .Select(article => article.Guid)
            .ToListAsync(cancellationToken);
    }
}
