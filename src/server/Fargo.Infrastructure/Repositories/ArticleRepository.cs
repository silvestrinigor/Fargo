using System.Linq.Expressions;
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

    // Whitelist of fields an article list may be sorted by, mapping each
    // query-string name to its key selector. Only scalar columns the database can
    // order by are included; requesting anything else is rejected with 400.
    private static readonly IReadOnlyDictionary<string, Expression<Func<Article, object?>>> ArticleSortFields =
        new Dictionary<string, Expression<Func<Article, object?>>>
        {
            ["guid"] = a => a.Guid,
            ["name"] = a => a.Name,
            ["description"] = a => a.Description,
            ["type"] = a => a.ArticleType,
            ["trackingmode"] = a => a.TrackingMode,
        };

    public async Task<PagedResult<ArticleDto>> GetManyInfoOrderedByGuidAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var articleQueryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions);

        articleQueryFiltered = ApplySearch(articleQueryFiltered, search);

        var totalCount = await articleQueryFiltered.CountAsync(cancellationToken);

        var articles = await articleQueryFiltered
            .ApplySort(sort, ArticleSortFields, a => a.Name, a => a.Guid)
            .WithPagination(pagination)
            .Select(ArticleDtoMapping.Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<ArticleDto>(
            articles,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    // Case-insensitive substring match on the article's name or description via
    // PostgreSQL ILIKE. The term's LIKE wildcards are escaped so a literal '%' or
    // '_' in the search text matches itself rather than acting as a wildcard.
    private static IQueryable<Article> ApplySearch(IQueryable<Article> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var pattern = $"%{SearchPattern.Escape(search.Trim())}%";

        return query.Where(a =>
            EF.Functions.ILike((string)a.Name, pattern, SearchPattern.EscapeChar)
            || EF.Functions.ILike((string)a.Description, pattern, SearchPattern.EscapeChar));
    }

    private static IQueryable<Article> ApplyPartitionFilter(
        IQueryable<Article> query,
        IReadOnlyCollection<Guid>? partitionGuids)
        => query.WherePartitionAccessible(
            partitionGuids,
            article => article.Partitions,
            partition => partition.PartitionGuid);

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

    public async Task<ArticleTrackingMode?> GetTrackingModeByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Articles.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var modes = await queryFiltered
            .Where(a => a.Guid == articleGuid)
            .Select(a => (ArticleTrackingMode?)a.TrackingMode)
            .ToListAsync(cancellationToken);

        return modes.FirstOrDefault();
    }

    public async Task<ArticleInventoryDto> GetEventInventoryInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        bool includeDescendents = true,
        DateTimeOffset? at = null,
        CancellationToken cancellationToken = default)
    {
        // A null upper bound means "all events"; coalesce to the maximum instant
        // so the SQL statement treats both cases uniformly.
        var upperBound = at ?? DateTimeOffset.MaxValue;

        if (insideItemContainerGuids is not { Count: > 0 })
        {
            // No container filter: the net stock of the article across the whole
            // system is the quantity that entered containers minus the quantity
            // that left them. A transfer between two containers contributes +q
            // (destination) and -q (origin) and so nets to zero, leaving only
            // receipts (from outside) and dispatches (to outside).
            FormattableString allQuery = $"""
            SELECT COALESCE(SUM(
                CASE WHEN moviment.to_item_container_guid IS NOT NULL THEN moviment.quantity ELSE 0 END
                -
                CASE WHEN moviment.from_item_container_guid IS NOT NULL THEN moviment.quantity ELSE 0 END
            ), 0) AS value
            FROM item_moviment_events AS moviment
            INNER JOIN item_events AS event
                ON event.guid = moviment.item_event_guid
            WHERE event.article_guid = {articleGuid}
              AND event.occured_at <= {upperBound}
            """;

            var allNet = await context.Database
                .SqlQuery<double>(allQuery)
                .ToListAsync(cancellationToken);

            return new ArticleInventoryDto(TotalCount: allNet.FirstOrDefault());
        }

        var containers = insideItemContainerGuids.Distinct().ToArray();

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
            WHERE {includeDescendents}
              AND child_article.article_type = {(int)ArticleType.Container}
        )
        SELECT COALESCE(SUM(
            CASE
                WHEN moviment.to_item_container_guid IN (SELECT guid FROM container_set) THEN moviment.quantity
                ELSE 0
            END
            -
            CASE
                WHEN moviment.from_item_container_guid IN (SELECT guid FROM container_set) THEN moviment.quantity
                ELSE 0
            END
        ), 0) AS value
        FROM item_moviment_events AS moviment
        INNER JOIN item_events AS event
            ON event.guid = moviment.item_event_guid
        WHERE event.article_guid = {articleGuid}
          AND event.occured_at <= {upperBound}
        """;

        var net = await context.Database
            .SqlQuery<double>(query)
            .ToListAsync(cancellationToken);

        return new ArticleInventoryDto(TotalCount: net.FirstOrDefault());
    }
}
