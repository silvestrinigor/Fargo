using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.Core.Partitions;
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
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var partition = await ApplyPartitionFilter(
                partitions
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .Select(PartitionDtoMappings.Projection)
            .SingleOrDefaultAsync(partition => partition.Guid == entityGuid, cancellationToken);

        return partition;
    }

    public async Task<IReadOnlyCollection<PartitionDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                partitions
                    .TemporalAsOfIfProvided(asOfDateTime)
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .OrderBy(partition => partition.Guid)
            .WithPagination(pagination)
            .Select(PartitionDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return result;
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
        bool? notChildOfAnyPartition
    )
    {
        if (partitionGuids is null)
        {
            if (notChildOfAnyPartition is true)
            {
                return query.Where(partition => partition.ParentPartitionGuid == null);
            }

            if (notChildOfAnyPartition is false)
            {
                return query.Where(article => article.ParentPartitionGuid != null);
            }

            return query;
        }

        if (notChildOfAnyPartition is true)
        {
            return query.Where(partition =>
                partition.ParentPartitionGuid == null ||
                partitionGuids.Contains(partition.ParentPartitionGuid.Value));
        }

        return query.Where(partition =>
            partition.ParentPartitionGuid != null &&
            partitionGuids.Contains(partition.ParentPartitionGuid.Value));
    }

}
