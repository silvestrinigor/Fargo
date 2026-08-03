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
    public void Add(Partition partition) => context.Partitions.Add(partition);

    public void Remove(Partition partition) => context.Partitions.Remove(partition);

    public Task<Partition?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
    {
        var partitionTask = context.Partitions
            .SingleOrDefaultAsync(partition => partition.Guid == entityGuid, cancellationToken);

        return partitionTask;
    }

    public Task<PartitionDto?> GetInfoByGuid(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Partitions.AsNoTracking(),
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition);

        var partitionTask = queryFiltered
            .Where(partition => partition.Guid == entityGuid)
            .Select(PartitionDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return partitionTask;
    }

    public async Task<IReadOnlyCollection<PartitionDto>> GetManyInfo(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
                context.Partitions
                    .AsNoTracking(),
                childOfAnyOfThesePartitions,
                notChildOfAnyPartition);

        var partition = await queryFiltered
            .OrderBy(partition => partition.Guid)
            .WithPagination(pagination)
            .Select(PartitionDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return partition;
    }

    public async Task<IReadOnlyCollection<Guid>> GetDescendantGuidsAsync(
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

    public async Task<IReadOnlyCollection<Guid>> GetDescendantGuidsAsync(
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
            var descendants = await GetDescendantGuidsAsync(
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

    public async Task<bool> HasAnyAssociatedEntityAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        if (await context.Partitions.AnyAsync(p => p.ParentPartitionGuid == partitionGuid, cancellationToken))
        {
            return true;
        }

        if (await context.Articles.AnyAsync(a => a.Partitions.Any(p => p.Guid == partitionGuid), cancellationToken))
        {
            return true;
        }

        if (await context.Users.AnyAsync(u => u.Partitions.Any(p => p.Guid == partitionGuid), cancellationToken))
        {
            return true;
        }

        if (await context.UserGroups.AnyAsync(u => u.Partitions.Any(p => p.Guid == partitionGuid), cancellationToken))
        {
            return true;
        }

        if (await context.Items.AnyAsync(i => i.Partitions.Any(p => p.Guid == partitionGuid), cancellationToken))
        {
            return true;
        }

        return false;
    }
}
