using Fargo.Application.Audits;
using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class AuditLogRepository(FargoDbContext context) : IAuditLogRepository, IAuditLogQueryRepository
{
    public void Add(AuditLog auditLog)
    {
        context.AuditLogs.Add(auditLog);
    }

    public async Task<AuditLogDto?> GetInfoByGuidAsync(
        Guid auditLogGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = await ApplyPartitionFilter(
                context.AuditLogs.AsNoTracking(),
                childOfAnyOfThesePartitions)
            .Where(a => a.Guid == auditLogGuid)
            .Include(a => a.Partitions)
            .SingleOrDefaultAsync(cancellationToken);

        return auditLog?.ToDto();
    }

    public async Task<PagedResult<AuditLogDto>> GetManyInfoOrderedByOccurredAtAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Guid? actorGuid = null,
        ActorType? actorType = null,
        Guid? entityGuid = null,
        EntityType? entityType = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.AuditLogs.AsNoTracking(),
            childOfAnyOfThesePartitions);

        if (actorGuid is not null)
        {
            queryFiltered = queryFiltered.Where(a => a.ActorGuid == actorGuid);
        }

        if (actorType is not null)
        {
            queryFiltered = queryFiltered.Where(a => a.ActorType == actorType);
        }

        if (entityGuid is not null)
        {
            queryFiltered = queryFiltered.Where(a => a.EntityGuid == entityGuid);
        }

        if (entityType is not null)
        {
            queryFiltered = queryFiltered.Where(a => a.EntityType == entityType);
        }

        if (periodStart is not null)
        {
            queryFiltered = queryFiltered.Where(a => a.OccurredAt >= periodStart);
        }

        if (periodEnd is not null)
        {
            queryFiltered = queryFiltered.Where(a => a.OccurredAt <= periodEnd);
        }

        var totalCount = await queryFiltered.CountAsync(cancellationToken);

        var auditLogs = await queryFiltered
        .OrderBy(a => a.OccurredAt)
        .Include(a => a.Partitions)
        .WithPagination(pagination)
        .ToListAsync(cancellationToken);

        var items = auditLogs.Select(a => a.ToDto()).ToList();

        return new PagedResult<AuditLogDto>(
            items,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    private static IQueryable<AuditLog> ApplyPartitionFilter(
        IQueryable<AuditLog> query,
        IReadOnlyCollection<Guid>? partitionGuids)
        => query.WherePartitionAccessible(
            partitionGuids,
            auditLog => auditLog.Partitions,
            partition => partition.PartitionGuid);
}
