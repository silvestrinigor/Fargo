using Fargo.Application;
using Fargo.Application.Items;
using Fargo.Core.Items;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemRepository(FargoDbContext context) : IItemRepository, IItemQueryRepository
{
    private readonly DbSet<Item> items = context.Items;

    public void Add(Item item) => items.Add(item);

    public void Remove(Item item) => items.Remove(item);

    public Task<Item?> GetByGuid(Guid entityGuid, CancellationToken cancellationToken = default)
        => items
            .Include(item => item.Article)
            .Include(item => item.Partitions)
            .SingleOrDefaultAsync(item => item.Guid == entityGuid, cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetContainerDescendantGuids(
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

    public async Task<ItemDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var item = await ApplyPartitionFilter(
                items
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .Where(item => item.Guid == entityGuid)
            .Select(ItemDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return item;
    }

    public async Task<IReadOnlyCollection<ItemDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                items
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(ItemDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return result;
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

public sealed class ItemMovementRepository(FargoDbContext context) : IItemMovementRepository
{
    private readonly DbSet<ItemMovement> movements = context.ItemMovements;

    public void Add(ItemMovement movement) => movements.Add(movement);
}
