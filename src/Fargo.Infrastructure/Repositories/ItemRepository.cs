using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ItemRepository(FargoDbContext context) : IItemRepository, IItemQueryRepository
{
    private readonly DbSet<Item> items = context.Items;

    public async Task<IReadOnlyCollection<PartitionInformation>?> GetPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? partitionFilter = null,
        CancellationToken cancellationToken = default)
    {
        if (!await items.AnyAsync(i => i.Guid == entityGuid, cancellationToken))
        {
            return null;
        }

        IQueryable<Partition> query = items
            .Where(i => i.Guid == entityGuid)
            .SelectMany(i => i.Partitions);

        if (partitionFilter is not null)
        {
            query = query.Where(p => partitionFilter.Contains(p.Guid));
        }

        return await query
            .AsNoTracking()
            .Select(PartitionMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public void Add(Item item)
    {
        context.Items.Add(item);
    }

    public void Remove(Item item)
    {
        context.Items.Remove(item);
    }

    public async Task<Item?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default)
    {
        return await items
            .Where(item => item.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ItemInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(item => item.Guid == entityGuid)
            .Select(ItemMappings.InformationProjection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ItemInformation>> GetManyInfo(
        Pagination pagination,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Item> query = items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        if (articleGuid.HasValue)
        {
            query = query.Where(item => item.Article.Guid == articleGuid.Value);
        }

        return await query
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(ItemMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetManyGuids(
        Pagination pagination,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Item> query = items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        if (articleGuid.HasValue)
        {
            query = query.Where(item => item.Article.Guid == articleGuid.Value);
        }

        return await query
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(item => item.Guid)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ItemInformation>> GetManyInfoWithNoPartition(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(i => !i.Partitions.Any())
            .OrderBy(i => i.Guid)
            .WithPagination(pagination)
            .Select(ItemMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<ItemInformation?> GetInfoByGuidInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return null;
        }

        var query = items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking();

        return await query
            .Where(item => item.Guid == entityGuid)
            .Where(item => item.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .Select(ItemMappings.InformationProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ItemInformation>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        IQueryable<Item> query = items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(item => item.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));

        if (articleGuid.HasValue)
        {
            query = query.Where(item => item.Article.Guid == articleGuid.Value);
        }

        return await query
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(ItemMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<ItemInformation?> GetInfoByGuidPublicOrInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(item => item.Guid == entityGuid)
            .Where(item => !item.Partitions.Any()
                || item.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)))
            .Select(ItemMappings.InformationProjection)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ItemInformation>> GetManyInfoInPartitionsOrPublic(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Item> query = items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(item => !item.Partitions.Any()
                || item.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));

        if (articleGuid.HasValue)
        {
            query = query.Where(item => item.Article.Guid == articleGuid.Value);
        }

        return await query
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(ItemMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetManyGuidsInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? articleGuid = null,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        if (partitionGuids == null || partitionGuids.Count == 0)
        {
            return [];
        }

        IQueryable<Item> query = items
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(item => item.Partitions.Any(partition => partitionGuids.Contains(partition.Guid)));

        if (articleGuid.HasValue)
        {
            query = query.Where(item => item.Article.Guid == articleGuid.Value);
        }

        return await query
            .OrderBy(item => item.Guid)
            .WithPagination(pagination)
            .Select(item => item.Guid)
            .ToListAsync(cancellationToken);
    }
}
