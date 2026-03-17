using Fargo.Application.Mappings;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class PartitionRepository(FargoDbContext context) : IPartitionRepository
{
    private readonly DbSet<Partition> partitions = context.Partitions;

    public void Add(Partition partition)
    {
        partitions.Add(partition);
    }

    public void Remove(Partition partition)
    {
        partitions.Remove(partition);
    }

    public async Task<Partition?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default)
    {
        return await partitions
            .Where(partition => partition.Guid == entityGuid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PartitionInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await partitions
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(partition => partition.Guid == entityGuid)
            .Select(PartitionMappings.InformationProjection)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PartitionInformation>> GetManyInfo(
        Pagination pagination,
        Guid? parentPartitionGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default)
    {
        return await partitions
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(p => parentPartitionGuid == null || p.ParentPartitionGuid == parentPartitionGuid)
            .OrderBy(partition => partition.Guid)
            .WithPagination(pagination)
            .Select(PartitionMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }
}
