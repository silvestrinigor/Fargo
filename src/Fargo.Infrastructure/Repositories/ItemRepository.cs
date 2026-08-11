using Fargo.Application;
using Fargo.Application.Items;
using Fargo.Application.Shared.Items;
using Fargo.Core.Items;
using Fargo.Core.Shared.Articles;
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
        .SingleOrDefaultAsync(item => item.Guid == entityGuid, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetContainedDescendantGuidsAsync(
        Guid itemContainerGuid,
        bool includeRoot = true,
        CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
    WITH item_tree AS
    (
        SELECT
            item.[Guid],
            item.[parent_item_container_guid],
            item.[article_guid]
        FROM [items] AS item
        INNER JOIN [articles] AS article
            ON article.[guid] = item.[article_guid]
        WHERE item.[guid] = {itemContainerGuid}
          AND article.[article_type] = {(int)ArticleType.Container}

        UNION ALL

        SELECT
            child.[guid],
            child.[parent_container_guid],
            child.[article_guid]
        FROM [items] AS child
        INNER JOIN item_tree AS parent
            ON child.[parent_item_container_guid] = parent.[Guid]
    )
    SELECT [Guid]
    FROM item_tree
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

    public Task<ItemDto?> GetInfoByGuid(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Items.AsNoTracking(),
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition);

        var itemTask = queryFiltered
            .Where(item => item.Guid == entityGuid)
            .Select(ItemDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return itemTask;
    }

    public async Task<IReadOnlyCollection<ItemDto>> GetManyInfo(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Items.AsNoTracking(),
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition);

        var item = await queryFiltered
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(ItemDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return item;
    }

    private static IQueryable<Item> ApplyPartitionFilter(
        IQueryable<Item> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notChildOfAnyPartition)
    {
        if (partitionGuids is null)
        {
            if (notChildOfAnyPartition is true)
            {
                return query.Where(item => !item.Partitions.Any());
            }

            if (notChildOfAnyPartition is false)
            {
                return query.Where(item => item.Partitions.Any());
            }

            return query;
        }

        if (notChildOfAnyPartition is true)
        {
            return query.Where(item =>
                !item.Partitions.Any() ||
                item.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
        }

        return query.Where(item =>
            item.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
    }
}
