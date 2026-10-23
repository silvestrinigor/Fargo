using Fargo.Application.Articles;
using Fargo.Application.Common;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
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
        CancellationToken cancellationToken = default)
    {
        var articleQueryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var article = await articleQueryFiltered
        .Where(article => article.Guid == articleGuid)
        .Select(ArticleDtoMapping.Projection)
        .SingleOrDefaultAsync(cancellationToken);

        return article;
    }

    public Task<ArticleDto?> GetInfoByBarcodeAsync(
        Barcode barcode,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var articleQueryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var articleTask = ApplyBarcodeFilter(articleQueryFiltered, barcode)
            .Select(ArticleDtoMapping.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return articleTask;
    }

    public async Task<PagedResult<ArticleDto>> GetManyInfoOrderedByGuidAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var articleQueryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var totalCount = await articleQueryFiltered.CountAsync(cancellationToken);

        var articles = await articleQueryFiltered
            .OrderBy(article => article.Guid)
            .WithPagination(pagination)
            .Select(ArticleDtoMapping.Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleDto>(
            articles,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    private static IQueryable<Article> ApplyPartitionFilter(
        IQueryable<Article> query,
        IReadOnlyCollection<Guid>? partitionGuids)
    {
        if (partitionGuids is null)
        {
            return query;
        }

        return query.Where(article =>
            article.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
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

    public async Task<ArticleInventoryDto> GetInventoryInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Items.Where(i => i.ArticleGuid == articleGuid);

        if (insideItemContainerGuids is { Count: > 0 })
        {
            query = query.Where(
                i => i.ParentItemContainerGuid != null &&
                insideItemContainerGuids.Contains(i.ParentItemContainerGuid.Value)
            );
        }

        var itemCount = await query.CountAsync(cancellationToken);

        return new ArticleInventoryDto(TotalCount: itemCount);
    }

    public async Task<ArticleInventoryDto> GetInventoryInfoByGuidWithDescendantsAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid> insideItemContainerGuids,
        CancellationToken cancellationToken = default)
    {
        // Count the article's items across the given containers and all of their
        // descendant containers, in one recursive query over the live container
        // tree. This replaces the previous approach of resolving every descendant
        // container GUID in the application and round-tripping them back as a
        // large IN list.
        //
        // Distinct guards against duplicate roots seeding the anchor more than
        // once. Only container-typed items are recursed into.
        var containers = insideItemContainerGuids.Distinct().ToArray();

        if (containers.Length == 0)
        {
            return new ArticleInventoryDto(TotalCount: 0);
        }

        FormattableString query = $"""
        WITH RECURSIVE container_set AS
        (
            SELECT guid
            FROM items
            WHERE guid = ANY({containers}::uuid[])

            UNION ALL

            SELECT child.guid
            FROM items AS child
            INNER JOIN container_set AS parent
                ON child.parent_item_container_guid = parent.guid
            INNER JOIN articles AS child_article
                ON child_article.guid = child.article_guid
            WHERE child_article.article_type = {(int)ArticleType.Container}
        )
        SELECT COUNT(*)::int AS value
        FROM items AS item
        INNER JOIN container_set AS cs
            ON cs.guid = item.parent_item_container_guid
        WHERE item.article_guid = {articleGuid}
        """;

        var counts = await context.Database
            .SqlQuery<int>(query)
            .ToListAsync(cancellationToken);

        return new ArticleInventoryDto(TotalCount: counts.FirstOrDefault());
    }

    public async Task<ArticleInventoryDto> GetInventoryInfoByGuidAtInstantAsync(
        Guid articleGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        bool includeDescendents = true,
        CancellationToken cancellationToken = default)
    {
        // Count the items of an article as they were located at a specific
        // instant, reconstructed from item_parent_container_history.
        //
        // When no containers are given, every item of the article that had a
        // containment record covering the instant is counted (i.e. existed then).
        //
        // When containers are given, the "container set" is those containers plus
        // (when descendants are included) every descendant container nested in
        // them at the instant. Items of the article whose historical parent at the
        // instant is in that set are counted. Only container-typed items are
        // recursed into, mirroring the live descendant walk.
        if (insideItemContainerGuids is not { Count: > 0 })
        {
            FormattableString allQuery = $"""
            -- COUNT(*) is safe (not COUNT(DISTINCT item_guid)): the non-overlap
            -- exclusion constraint guarantees at most one history row per item
            -- satisfies valid_period @> at, so each item is counted exactly once.
            SELECT COUNT(*)::int AS value
            FROM item_parent_container_history AS history
            INNER JOIN items AS item
                ON item.guid = history.item_guid
            WHERE item.article_guid = {articleGuid}
              AND history.valid_period @> {at}::timestamptz
            """;

            // ToListAsync executes the SQL as-is; SingleAsync would compose it as
            // a subquery, which is invalid for a trailing/CTE query.
            var allCounts = await context.Database
                .SqlQuery<int>(allQuery)
                .ToListAsync(cancellationToken);

            return new ArticleInventoryDto(TotalCount: allCounts.FirstOrDefault());
        }

        // Distinct guards against duplicate container guids in the input, which
        // would otherwise seed the recursion's anchor more than once and
        // double-count contents.
        var containers = insideItemContainerGuids.Distinct().ToArray();

        FormattableString query = $"""
        WITH RECURSIVE container_set AS
        (
            SELECT guid
            FROM items
            WHERE guid = ANY({containers}::uuid[])

            UNION ALL

            SELECT child.item_guid AS guid
            FROM container_set AS parent
            INNER JOIN item_parent_container_history AS child
                ON child.parent_item_container_guid = parent.guid
               AND child.valid_period @> {at}::timestamptz
            INNER JOIN items AS child_item
                ON child_item.guid = child.item_guid
            INNER JOIN articles AS child_article
                ON child_article.guid = child_item.article_guid
            WHERE {includeDescendents}
              AND child_article.article_type = {(int)ArticleType.Container}
        )
        -- COUNT(*) is safe here too: the non-overlap constraint means at most one
        -- history row per item matches valid_period @> at, and container_set
        -- contains distinct container guids, so no item is double-counted.
        SELECT COUNT(*)::int AS value
        FROM item_parent_container_history AS history
        INNER JOIN items AS item
            ON item.guid = history.item_guid
        INNER JOIN container_set AS cs
            ON cs.guid = history.parent_item_container_guid
        WHERE item.article_guid = {articleGuid}
          AND history.valid_period @> {at}::timestamptz
        """;

        var itemCounts = await context.Database
            .SqlQuery<int>(query)
            .ToListAsync(cancellationToken);

        return new ArticleInventoryDto(TotalCount: itemCounts.FirstOrDefault());
    }

    public Task<bool> ExistByGuidAsync(Guid articleGuid, IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null, CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var articleTask = queryFiltered
            .Where(x => x.Guid == articleGuid)
            .AnyAsync(cancellationToken);

        return articleTask;
    }
}
