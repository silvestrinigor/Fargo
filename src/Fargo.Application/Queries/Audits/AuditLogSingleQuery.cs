using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Dtos.Audits;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Queries.Audits;

public sealed record AuditLogSingleQuery(Guid AuditLogGuid) : IQuery<AuditLogDto?>;

public sealed class AuditLogSingleQueryHandler(
    ActorResolver actorResolver,
    IAuditLogQueryRepository auditLogQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<AuditLogSingleQuery, AuditLogDto?>
{
    public async Task<AuditLogDto?> HandleAsync(AuditLogSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorResolver.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var auditLog = await auditLogQueryRepository.GetInfoByGuidAsync(
            query.AuditLogGuid,
            actor.ToAuditLogAccessScope(),
            cancellationToken);

        return auditLog;
    }
}
