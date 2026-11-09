using Fargo.Application.Persistence.Repositories;
using Fargo.Application.Common;
using Fargo.Application.Operations.Audits.Dtos;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.Audits;

/// <summary>
/// Represents a query for retrieving a single audit log by its unique identifier.
/// </summary>
/// <param name="AuditLogGuid">The unique identifier of the audit log to retrieve.</param>
public sealed record AuditLogSingleQuery(Guid AuditLogGuid) : IQuery<AuditLogDto?>;

/// <summary>
/// Handles queries that retrieve a single audit log accessible to the current actor.
/// </summary>
/// <param name="actorResolver">Resolves the current actor and its partition access.</param>
/// <param name="auditLogQueryRepository">Provides access to audit log query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
public sealed class AuditLogSingleQueryHandler(
    ActorResolver actorResolver,
    IAuditLogQueryRepository auditLogQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<AuditLogSingleQuery, AuditLogDto?>
{
    /// <summary>
    /// Handles the AuditLogSingleQuery by retrieving the requested audit log if it
    /// is accessible to the current actor.
    /// </summary>
    /// <param name="query">The query containing the audit log identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The audit log DTO if found and accessible; otherwise, <see langword="null"/>.</returns>
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
