using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.Domain.Partitions;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class PartitionRepository(FargoDbContext context) : IPartitionRepository, IPartitionQueryRepository
{
    private readonly DbSet<Partition> partitions = context.Partitions;

    public void Add(Partition partition) => partitions.Add(partition);

    public void Remove(Partition partition) => partitions.Remove(partition);

    public Task<Partition?> GetByGuid(Guid entityGuid, CancellationToken cancellationToken = default)
        => partitions.SingleOrDefaultAsync(partition => partition.Guid == entityGuid, cancellationToken);

    public async Task<PartitionDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var partition = await ApplyPartitionFilter(
                partitions
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .SingleOrDefaultAsync(partition => partition.Guid == entityGuid, cancellationToken);

        return partition is null ? null : Map(partition);
    }

    public async Task<IReadOnlyCollection<PartitionDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                partitions
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                insideAnyOfThisPartitions,
                notInsideAnyPartition)
            .OrderBy(partition => partition.Guid)
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        return [.. result.Select(Map)];
    }

    public async Task<IReadOnlyCollection<Guid>> GetDescendantGuids(
        Guid partitionGuid,
        bool includeRoot = true,
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

        if (!includeRoot)
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
                cancellationToken);

            result.UnionWith(descendants);
        }

        return [.. result];
    }

    private static IQueryable<Partition> ApplyPartitionFilter(
        IQueryable<Partition> query,
        IReadOnlyCollection<Guid>? partitionGuids,
        bool? notInsideAnyPartition)
    {
        if (notInsideAnyPartition is true)
        {
            return query.Where(_ => false);
        }

        if (partitionGuids is not null)
        {
            query = query.Where(partition => partitionGuids.Contains(partition.Guid));
        }

        return query;
    }

    private static PartitionDto Map(Partition partition)
        => new(
            partition.Guid,
            partition.Name,
            partition.Description,
            partition.ParentPartitionGuid,
            partition.IsActive,
            partition.EditedByGuid);
}
