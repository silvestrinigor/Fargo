using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.Application.Shared.Partitions;
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

    public Task<Partition?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
        => partitions.SingleOrDefaultAsync(partition => partition.Guid == entityGuid, cancellationToken);

    public async Task<PartitionDto?> GetInfoByGuid(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var partition = await ApplyPartitionFilter(
                partitions
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition)
            .Where(partition => partition.Guid == entityGuid)
            .Select(PartitionDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return partition;
    }

    public async Task<IReadOnlyCollection<PartitionDto>> GetManyInfo(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ApplyPartitionFilter(
                partitions
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
        WITH RECURSIVE partition_tree AS
        (
            SELECT guid, parent_partition_guid
            FROM partitions
            WHERE guid = {partitionGuid}

            UNION ALL

            SELECT child.guid, child.parent_partition_guid
            FROM partitions AS child
            INNER JOIN partition_tree AS parent
                ON child.parent_partition_guid = parent.guid
        )
        SELECT guid
        FROM partition_tree
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
