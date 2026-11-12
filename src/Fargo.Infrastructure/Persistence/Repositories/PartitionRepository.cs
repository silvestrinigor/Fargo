using Fargo.Application.Common.Paging;
using Fargo.Application.Operations.Partitions;
using Fargo.Application.Operations.Partitions.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Partitions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Fargo.Infrastructure.Persistence.Repositories;

public sealed class PartitionRepository(FargoDbContext context) : IPartitionRepository, IPartitionQueryRepository
{
    public void Add(Partition partition) => context.Partitions.Add(partition);

    public void Remove(Partition partition) => context.Partitions.Remove(partition);

    // Fixed key for the single, global partition-hierarchy advisory lock. A cycle
    // can span several partitions whose only shared node is somewhere along their
    // paths, so a per-partition key does not reliably serialize the conflicting
    // re-parents. One global key serializes every re-parent, which is correct for
    // cycles of any size. This is cheap in practice because the caller only takes
    // the lock when assigning a parent partition, not on every partition update.
    private const long HierarchyLockKey = 0x_7A60_C017_A10C_2702;

    public async Task AcquireHierarchyLockAsync(CancellationToken cancellationToken = default)
    {
        // PostgreSQL transaction-scoped advisory lock: acquired for the given key,
        // released automatically when the surrounding transaction commits or rolls
        // back, so no manual unlock is needed. It must run inside a transaction
        // (the partition-update command runs under the transactional unit-of-work
        // decorator), otherwise the lock would release at the end of this single
        // statement.
        FormattableString query = $"SELECT pg_advisory_xact_lock({HierarchyLockKey})";

        await context.Database.ExecuteSqlInterpolatedAsync(query, cancellationToken);
    }

    public Task<Partition?> GetByGuidAsync(Guid entityGuid, CancellationToken cancellationToken = default)
    {
        return context.Partitions
        .SingleOrDefaultAsync(partition => partition.Guid == entityGuid, cancellationToken);
    }

    public Task<PartitionDto?> GetInfoByGuid(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.Partitions.AsNoTracking(),
            childOfAnyOfThesePartitions);

        return queryFiltered
        .Where(partition => partition.Guid == entityGuid)
        .Select(PartitionDtoMappings.Projection)
        .SingleOrDefaultAsync(cancellationToken);
    }

    private static readonly IReadOnlyDictionary<string, Expression<Func<Partition, object?>>> PartitionSortFields =
        new Dictionary<string, Expression<Func<Partition, object?>>>
        {
            ["guid"] = p => p.Guid,
            ["name"] = p => p.Name,
            ["description"] = p => p.Description,
        };

    public async Task<PagedResult<PartitionDto>> GetManyInfo(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
                context.Partitions
                    .AsNoTracking(),
                childOfAnyOfThesePartitions);

        queryFiltered = ApplySearch(queryFiltered, search);

        var totalCount = await queryFiltered.CountAsync(cancellationToken);

        var partitions = await queryFiltered
            .ApplySort(sort, PartitionSortFields, p => p.Name, p => p.Guid)
            .WithPagination(pagination)
            .Select(PartitionDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<PartitionDto>(
            partitions,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    private static IQueryable<Partition> ApplySearch(IQueryable<Partition> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var pattern = $"%{SearchPattern.Escape(search.Trim())}%";

        return query.Where(p =>
            EF.Functions.ILike((string)p.Name, pattern, SearchPattern.EscapeChar)
            || EF.Functions.ILike((string)p.Description, pattern, SearchPattern.EscapeChar));
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
        IReadOnlyCollection<Guid>? partitionGuids
    )
    {
        if (partitionGuids is null)
        {
            return query;
        }

        // A partition is visible when the actor has access to it directly (its
        // own GUID is in the access set) or when it is a child of an accessible
        // partition. The direct-access case is essential for root partitions
        // such as the global partition, whose ParentPartitionGuid is null and
        // which would otherwise never match the parent-based condition.
        return query.Where(partition =>
            partitionGuids.Contains(partition.Guid) ||
            (partition.ParentPartitionGuid != null &&
             partitionGuids.Contains(partition.ParentPartitionGuid.Value)));
    }

    public async Task<bool> HasChildrenAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        if (await context.Partitions.AnyAsync(p => p.ParentPartitionGuid == partitionGuid, cancellationToken))
        {
            return true;
        }

        return false;
    }
}
