using Fargo.Application;
using Fargo.Application.Items;
using Fargo.Domain.Items;
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
            .Include(item => item.Partitions)
            .SingleOrDefaultAsync(item => item.Guid == entityGuid, cancellationToken);

    public async Task<ItemDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var item = await ApplyPartitionFilter(
                items
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking()
                    .Include(item => item.Partitions),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .SingleOrDefaultAsync(item => item.Guid == entityGuid, cancellationToken);

        return item is null ? null : Map(item);
    }

    public async Task<IReadOnlyCollection<ItemDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                items
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking()
                    .Include(item => item.Partitions),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        return [.. result.Select(Map)];
    }

    private static IQueryable<Item> ApplyPartitionFilter(
        IQueryable<Item> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notInsideAnyPartition)
    {
        if (notInsideAnyPartition is true)
        {
            return query.Where(item => !item.Partitions.Any());
        }

        if (notInsideAnyPartition is false)
        {
            query = query.Where(item => item.Partitions.Any());
        }

        if (partitionGuids is { Count: > 0 })
        {
            query = query.Where(item =>
                !item.Partitions.Any() ||
                item.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));
        }

        return query;
    }

    private static ItemDto Map(Item item)
        => new(
            item.Guid,
            item.ArticleGuid,
            item.ProductionDate,
            [.. item.Partitions.Select(partition => partition.Guid)],
            item.EditedByGuid);
}
