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
                    .AsNoTracking(),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .Select(ArticleDtoMappings.Projection)
            .SingleOrDefaultAsync(article => article.Guid == entityGuid, cancellationToken);

        return article;
    }

    public async Task<ArticleDto?> GetInfoByBarcode(
        ArticleBarcodeDto articleBarcode,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyPartitionFilter(
            articles
                .TemporalAsOfIfProvided(asOfDateTime)
                .AsNoTracking(),
            insideAnyOfThisPartitions,
            notInsideAnyPartition);

        return await ApplyBarcodeFilter(query, articleBarcode)
            .Select(ArticleDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);
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
                    .AsNoTracking(),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .Select(ArticleDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return result;
    }

    private static IQueryable<Article> ApplyPartitionFilter(
        IQueryable<Article> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notInsideAnyPartition
    )
    {
        if (partitionGuids is null)
        {
            if (notInsideAnyPartition is true)
            {
                return query.Where(article => !article.Partitions.Any());
            }

            if (notInsideAnyPartition is false)
            {
                return query.Where(article => article.Partitions.Any());
            }

            return query;
        }

        if (notInsideAnyPartition is true)
        {
            return query.Where(article =>
                !article.Partitions.Any() ||
                article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
        }

        return query.Where(article =>
            article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
    }

    private static IQueryable<Article> ApplyBarcodeFilter(
        IQueryable<Article> query,
        ArticleBarcodeDto articleBarcode)
    {
        switch (articleBarcode.Type)
        {
            case BarcodeFormat.Ean13:
                {
                    var code = new Ean13(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.Ean13 == code);
                }
            case BarcodeFormat.Ean8:
                {
                    var code = new Ean8(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.Ean8 == code);
                }
            case BarcodeFormat.UpcA:
                {
                    var code = new UpcA(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.UpcA == code);
                }
            case BarcodeFormat.UpcE:
                {
                    var code = new UpcE(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.UpcE == code);
                }
            case BarcodeFormat.Code128:
                {
                    var code = new Code128(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.Code128 == code);
                }
            case BarcodeFormat.Code39:
                {
                    var code = new Code39(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.Code39 == code);
                }
            case BarcodeFormat.Itf14:
                {
                    var code = new Itf14(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.Itf14 == code);
                }
            case BarcodeFormat.Gs1128:
                {
                    var code = new Gs1128(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.Gs1128 == code);
                }
            case BarcodeFormat.QrCode:
                {
                    var code = new QrCode(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.QrCode == code);
                }
            case BarcodeFormat.DataMatrix:
                {
                    var code = new DataMatrix(articleBarcode.Barcode);
                    return query.Where(article => article.Barcodes.DataMatrix == code);
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(articleBarcode), articleBarcode.Type, "Unsupported barcode type.");
        }
    }

}
