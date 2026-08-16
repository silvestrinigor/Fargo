using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemRepository(FargoDbContext context) : IItemRepository, IItemQueryRepository
{
    public Task<Item?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
    {
        return context.Items
        .Include(item => item.Article)
        .Include(item => item.Partitions)
        .Include(item => item.ParentItemContainer)
        .SingleOrDefaultAsync(item => item.Guid == entityGuid, cancellationToken);
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

    public async Task<IReadOnlyCollection<ItemDto>> GetManyInfo(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Items.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var item = await queryFiltered
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(ItemDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return item;
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

    public async Task<IReadOnlyCollection<ItemDto>> GetLocationInfoByGuidAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
        WITH RECURSIVE item_location AS
        (
            SELECT
                item.guid,
                item.parent_item_container_guid
            FROM items AS item
            WHERE item.guid = {itemGuid}

            UNION ALL

            SELECT
                parent.guid,
                parent.parent_item_container_guid
            FROM items AS parent
            INNER JOIN item_location AS child
                ON parent.guid = child.parent_item_container_guid
        )
        SELECT guid
        FROM item_location;
        """;

        var guids = await context.Database
            .SqlQuery<Guid>(query)
            .ToListAsync(cancellationToken);

        var items = await context.Items
            .Where(i => guids.Contains(i.Guid))
            .Select(ItemDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return items;
    }
}
