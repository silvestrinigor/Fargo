using Fargo.Application;
using Fargo.Application.Items;
using Fargo.Application.Shared.Items;
using Fargo.Core.Items;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemRepository(FargoDbContext context) : IItemRepository, IItemQueryRepository
{
    public void Add(Item item) => context.Items.Add(item);

    public void Remove(Item item) => context.Items.Remove(item);

    public Task<Item?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
        => context.Items
            .Include(item => item.Article)
            .Include(item => item.Partitions)
            .SingleOrDefaultAsync(item => item.Guid == entityGuid, cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetContainerDescendantGuidsAsync(
        Guid itemGuid,
        bool includeRoot = true,
        CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
            WITH ItemContainerTree AS
            (
                SELECT [Guid], [ParentContainerGuid]
                FROM [Items]
                WHERE [Guid] = {itemGuid}

                UNION ALL

                SELECT child.[Guid], child.[ParentContainerGuid]
                FROM [Items] AS child
                INNER JOIN ItemContainerTree AS parent
                    ON child.[ParentContainerGuid] = parent.[Guid]
            )
            SELECT [Guid]
            FROM ItemContainerTree
            """;

        var guids = await context.Database
            .SqlQuery<Guid>(query)
            .ToListAsync(cancellationToken);

        if (!includeRoot)
        {
            guids.RemoveAll(guid => guid == itemGuid);
        }

        return guids;
    }

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
                item.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
        }

        return query.Where(item =>
            item.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
    }
}
