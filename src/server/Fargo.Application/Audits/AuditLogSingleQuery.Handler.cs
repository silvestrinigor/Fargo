using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Audits;

/// <summary>
/// Handles queries that retrieve a single audit log accessible to the current actor.
/// </summary>
/// <param name="actorResolver">Resolves the current actor and its partition access.</param>
/// <param name="auditLogQueryRepository">Provides access to audit log query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
/// <param name="logger">Logs the execution of the query.</param>
public sealed class AuditLogSingleQueryHandler(
    ActorResolver actorResolver,
    IAuditLogQueryRepository auditLogQueryRepository,
    ICurrentActor currentActor,
    ILogger<AuditLogSingleQueryHandler> logger
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
        using var scope = logger.BeginActorScope(currentActor);
        logger.QueryStarted("Audit log single query", "auditLog", query.AuditLogGuid);

        var actor = await actorResolver.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var auditLog = await auditLogQueryRepository.GetInfoByGuidAsync(
            query.AuditLogGuid,
            actor.PartitionAccessGuids,
            cancellationToken);

        logger.QueryCompleted("Audit log single query", "auditLog", query.AuditLogGuid, found: auditLog is not null);

        return auditLog;
    }
}
