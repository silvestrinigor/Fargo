using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Shared.Barcodes;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Articles;

public sealed class ArticleRepository(FargoDbContext context) : IArticleRepository, IArticleQueryRepository
{
    private readonly DbSet<Article> articles = context.Articles;

    private readonly DbSet<Item> items = context.Items;

    public void Add(Article article) => articles.Add(article);

    public void Remove(Article article) => articles.Remove(article);

    public Task<bool> HasItemsAssociatedAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        => items.AnyAsync(item => item.ArticleGuid == articleGuid, cancellationToken);

    public async Task<bool> IsDependenceOfAnotherArticle(Guid articleGuid, CancellationToken cancellationToken = default)
    {
        return await articles.AnyAsync(v => v.IsVariation && v.Variation.FromArticleGuid == articleGuid, cancellationToken)
            || await articles.AnyAsync(p => p.IsPack && p.Pack.FromArticleGuid == articleGuid, cancellationToken)
            || await articles.AnyAsync(k => k.IsKit && k.KitComponents.Any(c => c.FromArticleGuid == articleGuid), cancellationToken);
    }

    public Task<Article?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
        => articles
            .Include(article => article.Barcode)
            .Include(article => article.Partitions)
            .SingleOrDefaultAsync(article => article.Guid == entityGuid, cancellationToken);

    public Task<bool> ExistsByEan13Async(Ean13 code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.Ean13 == code, cancellationToken);

    public Task<bool> ExistsByEan8Async(Ean8 code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.Ean8 == code, cancellationToken);

    public Task<bool> ExistsByUpcEAsync(UpcE code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.UpcE == code, cancellationToken);

    public Task<bool> ExistsByUpcAAsync(UpcA code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.UpcA == code, cancellationToken);

    public Task<bool> ExistsByCode128Async(Code128 code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.Code128 == code, cancellationToken);

    public Task<bool> ExistsByCode39Async(Code39 code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.Code39 == code, cancellationToken);

    public Task<bool> ExistsByItf14Async(Itf14 code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.Itf14 == code, cancellationToken);

    public Task<bool> ExistsByGs1128Async(Gs1128 code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.Gs1128 == code, cancellationToken);

    public Task<bool> ExistsByQrCodeAsync(QrCode code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.QrCode == code, cancellationToken);

    public Task<bool> ExistsByDataMatrixAsync(DataMatrix code, CancellationToken cancellationToken = default)
        => articles.AnyAsync(article => article.Barcode.DataMatrix == code, cancellationToken);

    public async Task<ArticleDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var article = await ApplyPartitionFilter(
                articles
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .Where(article => article.Guid == entityGuid)
            .Select(ArticleDtoMapping.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return article;
    }

    public async Task<ArticleDto?> GetInfoByBarcodeAsync(
        Barcode barcode,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyPartitionFilter(
            articles
                .AsNoTracking(),
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition);

        return await ApplyBarcodeFilter(query, barcode)
            .Select(ArticleDtoMapping.Projection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ArticleDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                articles
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .Select(ArticleDtoMapping.Projection)
            .ToListAsync(cancellationToken);

        return result;
    }

    private static IQueryable<Article> ApplyPartitionFilter(
        IQueryable<Article> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notChildOfAnyPartition
    )
    {
        if (partitionGuids is null)
        {
            if (notChildOfAnyPartition is true)
            {
                return query.Where(article => !article.Partitions.Any());
            }

            if (notChildOfAnyPartition is false)
            {
                return query.Where(article => article.Partitions.Any());
            }

            return query;
        }

        if (notChildOfAnyPartition is true)
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
        Barcode barcode)
    {
        switch (barcode.Format)
        {
            case BarcodeFormat.Ean13:
                {
                    var code = Ean13.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.Ean13 == code);
                }
            case BarcodeFormat.Ean8:
                {
                    var code = Ean8.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.Ean8 == code);
                }
            case BarcodeFormat.UpcA:
                {
                    var code = UpcA.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.UpcA == code);
                }
            case BarcodeFormat.UpcE:
                {
                    var code = UpcE.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.UpcE == code);
                }
            case BarcodeFormat.Code128:
                {
                    var code = Code128.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.Code128 == code);
                }
            case BarcodeFormat.Code39:
                {
                    var code = Code39.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.Code39 == code);
                }
            case BarcodeFormat.Itf14:
                {
                    var code = Itf14.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.Itf14 == code);
                }
            case BarcodeFormat.Gs1128:
                {
                    var code = Gs1128.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.Gs1128 == code);
                }
            case BarcodeFormat.QrCode:
                {
                    var code = QrCode.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.QrCode == code);
                }
            case BarcodeFormat.DataMatrix:
                {
                    var code = DataMatrix.FromBarcode(barcode);
                    return query.Where(article => article.Barcode.DataMatrix == code);
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(barcode), barcode.Format, "Unsupported barcode type.");
        }
    }
}
