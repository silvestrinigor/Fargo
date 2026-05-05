using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Items;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ArticleRepository(FargoDbContext context) : IArticleRepository, IArticleQueryRepository
{
    private readonly DbSet<Article> articles = context.Articles;
    private readonly DbSet<Item> items = context.Items;

    public void Add(Article article) => articles.Add(article);

    public void Remove(Article article) => articles.Remove(article);

    public Task<bool> HasItemsAssociated(Guid articleGuid, CancellationToken cancellationToken = default)
        => items.AnyAsync(item => item.ArticleGuid == articleGuid, cancellationToken);

    public Task<Article?> GetByGuid(Guid entityGuid, CancellationToken cancellationToken = default)
        => articles
            .Include(article => article.Partitions)
            .SingleOrDefaultAsync(article => article.Guid == entityGuid, cancellationToken);

    public Task<bool> ExistsByBarcode(Ean13 code)
        => articles.AnyAsync(article => article.Barcodes.Ean13 == code);

    public Task<bool> ExistsByBarcode(Ean8 code)
        => articles.AnyAsync(article => article.Barcodes.Ean8 == code);

    public Task<bool> ExistsByBarcode(UpcE code)
        => articles.AnyAsync(article => article.Barcodes.UpcE == code);

    public Task<bool> ExistsByBarcode(UpcA code)
        => articles.AnyAsync(article => article.Barcodes.UpcA == code);

    public Task<bool> ExistsByBarcode(Code128 code)
        => articles.AnyAsync(article => article.Barcodes.Code128 == code);

    public Task<bool> ExistsByBarcode(Code39 code)
        => articles.AnyAsync(article => article.Barcodes.Code39 == code);

    public Task<bool> ExistsByBarcode(Itf14 code)
        => articles.AnyAsync(article => article.Barcodes.Itf14 == code);

    public Task<bool> ExistsByBarcode(Gs1128 code)
        => articles.AnyAsync(article => article.Barcodes.Gs1128 == code);

    public Task<bool> ExistsByBarcode(QrCode code)
        => articles.AnyAsync(article => article.Barcodes.QrCode == code);

    public Task<bool> ExistsByBarcode(DataMatrix code)
        => articles.AnyAsync(article => article.Barcodes.DataMatrix == code);

    public async Task<ArticleDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var article = await ApplyPartitionFilter(
                articles
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking()
                    .Include(article => article.Partitions),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .SingleOrDefaultAsync(article => article.Guid == entityGuid, cancellationToken);

        return article is null ? null : Map(article);
    }

    public async Task<IReadOnlyCollection<ArticleDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                articles
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking()
                    .Include(article => article.Partitions),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        return [.. result.Select(Map)];
    }

    private static IQueryable<Article> ApplyPartitionFilter(
        IQueryable<Article> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notInsideAnyPartition
    )
    {
        if (notInsideAnyPartition is true)
        {
            return query.Where(article => !article.Partitions.Any());
        }

        if (notInsideAnyPartition is false)
        {
            query = query.Where(article => article.Partitions.Any());
        }

        if (partitionGuids is not null)
        {
            query = query.Where(article =>
                article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
        }

        return query;
    }

    private static ArticleDto Map(Article article)
        => new(
            article.Guid,
            article.Name,
            article.Description,
            article.ShelfLife,
            new ArticleMetricsDto(
                article.Metrics.Mass,
                article.Metrics.LengthX,
                article.Metrics.LengthY,
                article.Metrics.LengthZ),
            new ArticleBarcodesDto(
                article.Barcodes.Ean13,
                article.Barcodes.Ean8,
                article.Barcodes.UpcA,
                article.Barcodes.UpcE,
                article.Barcodes.Code128,
                article.Barcodes.Code39,
                article.Barcodes.Itf14,
                article.Barcodes.Gs1128,
                article.Barcodes.QrCode,
                article.Barcodes.DataMatrix),
            [.. article.Partitions.Select(partition => partition.Guid)],
            article.IsActive,
            article.EditedByGuid);
}
