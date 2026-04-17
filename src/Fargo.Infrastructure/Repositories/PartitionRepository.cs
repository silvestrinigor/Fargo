using Fargo.Application.Mappings;
using Fargo.Domain;
using Fargo.Domain.Partitions;
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
        Guid? parentPartitionGuid = null,
        DateTimeOffset? asOfDateTime = null,
        bool rootOnly = false,
        CancellationToken cancellationToken = default)
    {
        return await partitions
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(partition =>
                !rootOnly || partition.ParentPartitionGuid == null)
            .Where(partition =>
                parentPartitionGuid == null ||
                partition.ParentPartitionGuid == parentPartitionGuid)
            .OrderBy(partition => partition.Guid)
            .WithPagination(pagination)
            .Select(PartitionMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PartitionInformation>> GetManyInfoByGuids(
        IReadOnlyCollection<Guid> partitionGuids,
        Pagination pagination,
        Guid? parentPartitionGuid = null,
        DateTimeOffset? asOfDateTime = null,
        bool rootOnly = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionGuids);

        if (partitionGuids.Count == 0)
        {
            return [];
        }

        return await partitions
            .TemporalAsOfIfProvided(asOfDateTime)
            .AsNoTracking()
            .Where(partition => partitionGuids.Contains(partition.Guid))
            .Where(partition =>
                !rootOnly || partition.ParentPartitionGuid == null)
            .Where(partition =>
                parentPartitionGuid == null ||
                partition.ParentPartitionGuid == parentPartitionGuid)
            .OrderBy(partition => partition.Guid)
            .WithPagination(pagination)
            .Select(PartitionMappings.InformationProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
        Guid partitionGuid,
        bool includeSelf = true,
        CancellationToken cancellationToken = default)
    {
        FormattableString query = $"""
            WITH PartitionTree AS
            (
                SELECT [Guid], [ParentPartitionGuid]
                FROM [Partitions]
                WHERE [Guid] = {partitionGuid}

                UNION ALL

                SELECT child.[Guid], child.[ParentPartitionGuid]
                FROM [Partitions] AS child
                INNER JOIN PartitionTree AS parent
                    ON child.[ParentPartitionGuid] = parent.[Guid]
            )
            SELECT [Guid]
            FROM PartitionTree
            """;

        var guids = await context.Database
            .SqlQuery<Guid>(query)
            .ToListAsync(cancellationToken);

        if (!includeSelf)
        {
            guids.RemoveAll(guid => guid == partitionGuid);
        }

        return guids;
    }

    public async Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
        IReadOnlyCollection<Guid> partitionGuids,
        bool includeRoots = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partitionGuids);

        if (partitionGuids.Count == 0)
        {
            return [];
        }

        var result = new HashSet<Guid>();

        foreach (var partitionGuid in partitionGuids.Distinct())
        {
            var descendants = await GetDescendantGuids(
                partitionGuid,
                includeRoots,
                cancellationToken
            );

            result.UnionWith(descendants);
        }

        return [.. result];
    }
}
