using Fargo.Application.Common.Paging;
using Fargo.Application.Operations.Audits;
using Fargo.Application.Operations.Audits.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Audits;
using Fargo.Core.Entities.Actors;
using Microsoft.EntityFrameworkCore;


namespace Fargo.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(FargoDbContext context) : IAuditLogRepository, IAuditLogQueryRepository
{
    public void Add(AuditLog auditLog)
    {
        context.AuditLogs.Add(auditLog);
    }

    public async Task<AuditLogDto?> GetInfoByGuidAsync(
        Guid auditLogGuid,
        AuditLogAccessScope accessScope,
        CancellationToken cancellationToken = default)
    {
        var auditLog = await ApplyAccessScope(context, context.AuditLogs.AsNoTracking(), accessScope)
            .Where(a => a.Guid == auditLogGuid)
            .SingleOrDefaultAsync(cancellationToken);

        return auditLog?.ToDto();
    }
    public async Task<PagedResult<AuditLogDto>> GetManyInfoOrderedByOccurredAtAsync(
        Pagination pagination,
        AuditLogAccessScope accessScope,
        Guid? actorGuid = null,
        ActorType? actorType = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyAccessScope(context, context.AuditLogs.AsNoTracking(), accessScope);

        if (actorGuid is not null)
        {
            queryFiltered = queryFiltered.Where(a => a.ActorGuid == actorGuid);
        }

        if (actorType is not null)
        {
            queryFiltered = queryFiltered.Where(a => a.ActorType == actorType);
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
            .WithPagination(pagination)
            .ToListAsync(cancellationToken);

        var items = auditLogs.Select(a => a.ToDto()).ToList();

        return new PagedResult<AuditLogDto>(
            items,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    // Restricts the query to audits of actors the reader can access. A reader
    // with global-partition access sees everything. Otherwise an audit is visible
    // when it was recorded by the reader itself, or by an actor that belongs to
    // one of the reader's accessible partitions. The accessible actors are
    // expressed as a subquery over the actors/actor_partitions so the whole
    // filter stays a single translated SQL statement (no actor set round-trip).
    private static IQueryable<AuditLog> ApplyAccessScope(
        FargoDbContext context,
        IQueryable<AuditLog> query,
        AuditLogAccessScope accessScope)
    {
        if (accessScope.HasGlobalAccess)
        {
            return query;
        }

        var selfActorGuid = accessScope.SelfActorGuid;
        var accessiblePartitionGuids = accessScope.AccessiblePartitionGuids;

        var accessibleActorGuids = context.Actors
            .Where(actor =>
                actor.Guid == selfActorGuid
                || actor.Partitions.Any(p => accessiblePartitionGuids.Contains(p.PartitionGuid)))
            .Select(actor => actor.Guid);

        return query.Where(a => accessibleActorGuids.Contains(a.ActorGuid));
    }
}
