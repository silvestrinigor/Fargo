using Fargo.Application;
using Fargo.Application.Articles;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Articles;
using Fargo.Core.Shared.Barcodes;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ArticleRepository(FargoDbContext context) : IArticleRepository, IArticleQueryRepository
{
    public Task<Article?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
    {
        return context.Articles
        .Include(article => article.Variation)
        .Include(article => article.Pack)
        .Include(article => article.KitComponents)
        .Include(article => article.Container)
        .Include(article => article.Barcode)
        .Include(article => article.Dimension)
        .Include(article => article.Partitions)
        .SingleOrDefaultAsync(article => article.Guid == entityGuid, cancellationToken);
    }

    public Task<bool> HasItemsAssociatedAsync(Guid articleGuid, CancellationToken cancellationToken = default)
    {
        return context.Items
        .AnyAsync(item => item.ArticleGuid == articleGuid, cancellationToken);
    }

    public Task<bool> IsDependencyOfAnotherArticleAsync(Guid articleGuid, CancellationToken cancellationToken = default)
    {
        return context.Articles
        .AnyAsync(a =>
            (a.Variation != null && a.Variation.FromArticleGuid == articleGuid)
            || (a.Pack != null && a.Pack.FromArticleGuid == articleGuid)
            || (a.KitComponents != null && a.KitComponents.Any(c => c.FromArticleGuid == articleGuid)),
            cancellationToken);
    }

    public Task<bool> ExistsByEan13Async(Ean13 ean13, CancellationToken cancellationToken = default)
    {
        return context.Articles
        .AnyAsync(a => a.Barcode.Ean13 == ean13, cancellationToken);
    }

    public void Add(Article article)
    {
        context.Articles.Add(article);
    }

    public void Remove(Article article)
    {
        context.Articles.Remove(article);
    }

    public async Task<ArticleDto?> GetInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var articleQueryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition);

        var article = await articleQueryFiltered
        .Where(article => article.Guid == articleGuid)
        .Select(ArticleDtoMapping.Projection)
        .SingleOrDefaultAsync(cancellationToken);

        return article;
    }

    public Task<ArticleDto?> GetInfoByBarcodeAsync(
        Barcode barcode,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var articleQueryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition);

        var articleTask = ApplyBarcodeFilter(articleQueryFiltered, barcode)
            .Select(ArticleDtoMapping.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return articleTask;
    }

    public async Task<IReadOnlyCollection<ArticleDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var articleQueryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition);

        var article = await articleQueryFiltered
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .Select(ArticleDtoMapping.Projection)
            .ToListAsync(cancellationToken);

        return article;
    }

    private static IQueryable<Article> ApplyPartitionFilter(
        IQueryable<Article> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notChildOfAnyPartition)
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

    private static IQueryable<Article> ApplyBarcodeFilter(IQueryable<Article> query, Barcode barcode)
    {

        return barcode.BarcodeFormat switch
        {
            BarcodeFormat.Ean13 =>
                query.Where(a => a.Barcode.Ean13 == (Ean13)barcode.Value),

            _ => throw new ArgumentOutOfRangeException(
                nameof(barcode),
                barcode.BarcodeFormat,
                "Unsupported barcode format.")
        };
    }
}
