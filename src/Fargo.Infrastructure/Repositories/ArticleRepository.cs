using Fargo.Application.Articles;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Items;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public class ArticleRepository(FargoDbContext context) : IArticleRepository, IArticleQueryRepository
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
            .WithBarcodes()
            .Where(a => a.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ArticleDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        var article = await articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Include(a => a.Partitions)
            .WithBarcodes()
            .Where(article => article.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);

        return article?.ToInformation();
    }

    public async Task<IReadOnlyCollection<ArticleDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Include(a => a.Partitions)
            .WithBarcodes()
            .Where(a => search == null || EF.Functions.Like(a.Name, $"%{search}%"))
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        return [.. result.Select(a => a.ToInformation())];
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

    public async Task<ArticleDto?> GetInfoByGuidInPartitions(
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

        var article = await query
            .WithBarcodes()
            .Where(article => article.Guid == entityGuid)
            .Where(article => article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .FirstOrDefaultAsync(cancellationToken);

        return article?.ToInformation();
    }

    public async Task<IReadOnlyCollection<ArticleDto>> GetManyInfoWithNoPartition(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Include(a => a.Partitions)
            .WithBarcodes()
            .Where(a => !a.Partitions.Any())
            .Where(a => search == null || EF.Functions.Like(a.Name, $"%{search}%"))
            .OrderBy(a => a.Guid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        return [.. result.Select(a => a.ToInformation())];
    }

    public async Task<IReadOnlyCollection<ArticleDto>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        var query = articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        var result = await query
            .WithBarcodes()
            .Where(article => search == null || EF.Functions.Like(article.Name, $"%{search}%"))
            .Where(article => article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        return [.. result.Select(a => a.ToInformation())];
    }

    public async Task<ArticleDto?> GetInfoByGuidPublicOrInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        var article = await articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Include(a => a.Partitions)
            .WithBarcodes()
            .Where(article => article.Guid == entityGuid)
            .Where(article => !article.Partitions.Any()
                || article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .FirstOrDefaultAsync(cancellationToken);

        return article?.ToInformation();
    }

    public async Task<IReadOnlyCollection<ArticleDto>> GetManyInfoInPartitionsOrPublic(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await articles
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Include(a => a.Partitions)
            .WithBarcodes()
            .Where(article => search == null || EF.Functions.Like(article.Name, $"%{search}%"))
            .Where(article => !article.Partitions.Any()
                || article.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        return [.. result.Select(a => a.ToInformation())];
    }

    public async Task<(BarcodeFormat Format, string Code)?> FindConflictingBarcode(
        ArticleBarcodes barcodes,
        Guid excludeArticleGuid,
        CancellationToken cancellationToken = default)
    {
        if (barcodes.Ean13 is { } ean13 && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.Ean13 == ean13, cancellationToken))
        {
            return (BarcodeFormat.Ean13, ean13.Code);
        }

        if (barcodes.Ean8 is { } ean8 && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.Ean8 == ean8, cancellationToken))
        {
            return (BarcodeFormat.Ean8, ean8.Code);
        }

        if (barcodes.UpcA is { } upcA && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.UpcA == upcA, cancellationToken))
        {
            return (BarcodeFormat.UpcA, upcA.Code);
        }

        if (barcodes.UpcE is { } upcE && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.UpcE == upcE, cancellationToken))
        {
            return (BarcodeFormat.UpcE, upcE.Code);
        }

        if (barcodes.Code128 is { } code128 && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.Code128 == code128, cancellationToken))
        {
            return (BarcodeFormat.Code128, code128.Code);
        }

        if (barcodes.Code39 is { } code39 && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.Code39 == code39, cancellationToken))
        {
            return (BarcodeFormat.Code39, code39.Code);
        }

        if (barcodes.Itf14 is { } itf14 && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.Itf14 == itf14, cancellationToken))
        {
            return (BarcodeFormat.Itf14, itf14.Code);
        }

        if (barcodes.Gs1128 is { } gs1128 && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.Gs1128 == gs1128, cancellationToken))
        {
            return (BarcodeFormat.Gs1128, gs1128.Code);
        }

        if (barcodes.QrCode is { } qrCode && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.QrCode == qrCode, cancellationToken))
        {
            return (BarcodeFormat.QrCode, qrCode.Code);
        }

        if (barcodes.DataMatrix is { } dataMatrix && await articles.AnyAsync(a => a.Guid != excludeArticleGuid && a.Barcodes.DataMatrix == dataMatrix, cancellationToken))
        {
            return (BarcodeFormat.DataMatrix, dataMatrix.Code);
        }

        return null;
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

file static class ArticleQueryExtensions
{
    // Barcodes are columns on the Articles table — no separate Include needed.
    internal static IQueryable<Article> WithBarcodes(this IQueryable<Article> query) => query;
}
