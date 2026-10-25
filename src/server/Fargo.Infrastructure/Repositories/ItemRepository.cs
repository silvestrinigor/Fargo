using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemRepository(FargoDbContext context) : IItemRepository, IItemQueryRepository
{
    public Task<Item?> GetByGuidAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        return context.Items
        .Include(item => item.Article)
        .Include(item => item.Partitions)
        .Include(item => item.ParentItemContainer)
        .SingleOrDefaultAsync(item => item.Guid == itemGuid, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetContainedDescendantGuidsAsync(
        Guid itemContainerGuid,
        bool includeRoot = true,
        CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
        WITH RECURSIVE item_tree AS
        (
            SELECT
                item.guid,
                item.parent_item_container_guid,
                item.article_guid
            FROM items AS item
            INNER JOIN articles AS article
                ON article.guid = item.article_guid
            WHERE item.guid = {itemContainerGuid}
            AND article.article_type = {(int)ArticleType.Container}

            UNION ALL

            SELECT
                child.guid,
                child.parent_item_container_guid,
                child.article_guid
            FROM items AS child
            INNER JOIN item_tree AS parent
                ON child.parent_item_container_guid = parent.guid
        )
        SELECT guid
        FROM item_tree;
        """;

        var guids = await context.Database
            .SqlQuery<Guid>(query)
            .ToListAsync(cancellationToken);

        if (!includeRoot)
        {
            guids.Remove(itemContainerGuid);
        }

        return guids;
    }

    public void Add(Item item) => context.Items.Add(item);

    public void Remove(Item item) => context.Items.Remove(item);

    public Task<ItemDto?> GetInfoByGuidAsync(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Items.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var itemTask = queryFiltered
            .Where(item => item.Guid == entityGuid)
            .Select(ItemDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return itemTask;
    }

    public async Task<PagedResult<ItemDto>> GetManyInfoOrderedByGuidAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Items.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var totalCount = await queryFiltered.CountAsync(cancellationToken);

        var items = await queryFiltered
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(ItemDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<ItemDto>(
            items,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    private static IQueryable<Item> ApplyPartitionFilter(
        IQueryable<Item> query,
        IReadOnlyCollection<Guid>? partitionGuids)
    {
        if (partitionGuids is null)
        {
            return query;
        }

        return query.Where(item =>
            item.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
    }

    public async Task<IReadOnlyCollection<ItemDto>> GetLocationInfoByGuidOrderedByDepthAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
        WITH RECURSIVE item_location AS
        (
            SELECT
                item.guid,
                item.parent_item_container_guid,
                0 AS depth
            FROM items AS item
            WHERE item.guid = {itemGuid}

            UNION ALL

            SELECT
                parent.guid,
                parent.parent_item_container_guid,
                child.depth + 1
            FROM items AS parent
            INNER JOIN item_location AS child
                ON parent.guid = child.parent_item_container_guid
        )
        SELECT guid
        FROM item_location
        ORDER BY depth;
        """;

        var guids = await context.Database
        .SqlQuery<Guid>(query)
        .ToListAsync(cancellationToken);

        var items = await context.Items
        .Where(i => guids.Contains(i.Guid))
        .Select(ItemDtoMappings.Projection)
        .ToListAsync(cancellationToken);

        // The IN-based projection does not preserve the depth order produced by
        // the recursive query, so reorder the results to match: item first,
        // outward to the root container last.
        var orderByGuid = guids
            .Select((guid, index) => (guid, index))
            .ToDictionary(x => x.guid, x => x.index);

        return [.. items.OrderBy(item => item.Guid is { } g ? orderByGuid[g] : int.MaxValue)];
    }

    public async Task<IReadOnlyCollection<ItemDto>> GetLocationInfoByGuidAtInstantOrderedByDepthAsync(
        Guid itemGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        // Walk the containment chain as it existed at the given instant.
        //
        // For each item in the chain, the parent is taken from the row of
        // item_parent_container_history whose validity period contains the
        // instant (valid_period @> at). Starting from the queried item, the
        // recursion follows that historical parent outward until an item has no
        // historical parent (it was a root at that instant) or no history row
        // covering the instant (the chain ends).
        FormattableString query = $"""
        WITH RECURSIVE item_location AS
        (
            SELECT
                history.item_guid AS guid,
                history.parent_item_container_guid,
                0 AS depth
            FROM item_parent_container_history AS history
            WHERE history.item_guid = {itemGuid}
            AND history.valid_period @> {at}::timestamptz

            UNION ALL

            SELECT
                parent_history.item_guid AS guid,
                parent_history.parent_item_container_guid,
                child.depth + 1
            FROM item_parent_container_history AS parent_history
            INNER JOIN item_location AS child
                ON parent_history.item_guid = child.parent_item_container_guid
            WHERE parent_history.valid_period @> {at}::timestamptz
        )
        SELECT guid
        FROM item_location
        ORDER BY depth;
        """;

        var guids = await context.Database
        .SqlQuery<Guid>(query)
        .ToListAsync(cancellationToken);

        var items = await context.Items
        .Where(i => guids.Contains(i.Guid))
        .Select(ItemDtoMappings.Projection)
        .ToListAsync(cancellationToken);

        // Preserve the depth ordering produced by the recursive query, since the
        // subsequent projection does not guarantee it.
        var orderByGuid = guids
            .Select((guid, index) => (guid, index))
            .ToDictionary(x => x.guid, x => x.index);

        return [.. items.OrderBy(item => item.Guid is { } g ? orderByGuid[g] : int.MaxValue)];
    }

    public async Task<IReadOnlyCollection<ItemMovimentDto>?> GetItemMovimentsInfoByGuidOrderedByOccurredAtAsync(Guid itemGuid, IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null, CancellationToken cancellationToken = default)
    {
        var itemMoviments = await context.Items
        .AsNoTracking()
        .Include(i => i.Moviments)
        .Where(i => i.Guid == itemGuid)
        .Select(i => i.Moviments)
        .FirstOrDefaultAsync(cancellationToken);

        if (itemMoviments is null)
        {
            return null;
        }

        return [.. itemMoviments.OrderBy(m => m.OccurredAt).Select(a => a.ToDto())];
    }

    public async Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetInventoryInfoByGuidAsync(
        IReadOnlyCollection<Guid>? itemContainerGuids = null,
        IReadOnlyCollection<Guid>? articleGuids = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Items.AsQueryable();

        if (itemContainerGuids is { Count: > 0 })
        {
            query = query.Where(x =>
                x.ParentItemContainerGuid != null &&
                itemContainerGuids.Contains(x.ParentItemContainerGuid.Value)
            );
        }

        if (articleGuids is { Count: > 0 })
        {
            query = query.Where(i => articleGuids.Contains(i.ArticleGuid));
        }

        // Group on the server and count per article. EF Core cannot translate the
        // LINQ CountBy operator to SQL, so use GroupBy + Count(), which maps to a
        // GROUP BY ... COUNT(*) query.
        return await query
            .GroupBy(item => item.ArticleGuid)
            .Select(group => new ItemContainerInventoryDto(group.Key, group.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetInventoryInfoByGuidAtInstantAsync(
        Guid itemContainerGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? articleGuids = null,
        bool includeDescendents = true,
        CancellationToken cancellationToken = default)
    {
        // Reconstruct the inventory of a container as it was at a specific instant.
        //
        // Containment at the instant is taken from item_parent_container_history
        // (the history row whose validity period contains the instant). The
        // "container set" is the set of container items whose direct contents are
        // counted: when descendants are included it is the root container plus
        // every descendant container that was nested in it at the instant;
        // otherwise it is just the root container. Items whose historical parent
        // is in the container set are then grouped by article and counted.
        //
        // Only container-typed items are recursed into, mirroring the live
        // descendant walk; all contained items are counted regardless of type.
        var articleFilter = articleGuids is { Count: > 0 }
            ? articleGuids.ToArray()
            : null;

        FormattableString query = $"""
        WITH RECURSIVE container_set AS
        (
            SELECT {itemContainerGuid} AS guid

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
        ),
        contained AS
        (
            SELECT contained_item.guid, contained_item.article_guid
            FROM item_parent_container_history AS history
            INNER JOIN container_set AS cs
                ON cs.guid = history.parent_item_container_guid
            INNER JOIN items AS contained_item
                ON contained_item.guid = history.item_guid
            WHERE history.valid_period @> {at}::timestamptz
              AND ({articleFilter}::uuid[] IS NULL OR contained_item.article_guid = ANY({articleFilter}::uuid[]))
        )
        SELECT
            article_guid AS article_guid,
            COUNT(*)::int AS total_count
        FROM contained
        GROUP BY article_guid;
        """;

        var rows = await context.Database
            .SqlQuery<InventoryRow>(query)
            .ToListAsync(cancellationToken);

        return [.. rows.Select(r => new ItemContainerInventoryDto(r.ArticleGuid, r.TotalCount))];
    }

    private sealed record InventoryRow(Guid ArticleGuid, int TotalCount);

    public Task<bool> ExistByGuidAsync(Guid itemGuid, IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null, CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Items.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var itemTask = queryFiltered
            .Where(item => item.Guid == itemGuid)
            .AnyAsync(cancellationToken);

        return itemTask;
    }
}
