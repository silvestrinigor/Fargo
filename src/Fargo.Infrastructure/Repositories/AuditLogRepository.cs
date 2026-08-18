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

    public async Task<IReadOnlyCollection<AuditLogDto>> GetManyInfoOrderedByOccurredAtAsync(
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

        var auditLogs = await queryFiltered
        .OrderBy(a => a.OccurredAt)
        .Include(a => a.Partitions)
        .WithPagination(pagination)
        .ToListAsync(cancellationToken);

        return [.. auditLogs.Select(a => a.ToDto())];
    }

    private static IQueryable<AuditLog> ApplyPartitionFilter(
        IQueryable<AuditLog> query,
        IReadOnlyCollection<Guid>? partitionGuids)
    {
        if (partitionGuids is null)
        {
            return query;
        }

        return query.Where(auditLog =>
            auditLog.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
    }
}
