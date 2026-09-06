using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Audits;

/// <summary>
/// Handles queries that retrieve a paginated collection of audit logs accessible
/// to the current actor.
/// </summary>
/// <param name="actorResolver">Resolves the current actor and its partition access.</param>
/// <param name="auditLogQueryRepository">Provides access to audit log query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
/// <param name="logger">Logs the execution of the query.</param>
public sealed class AuditLogsQueryHandler(
    ActorResolver actorResolver,
    IAuditLogQueryRepository auditLogQueryRepository,
    ICurrentActor currentActor,
    ILogger<AuditLogsQueryHandler> logger
) : IQueryHandler<AuditLogsQuery, IReadOnlyCollection<AuditLogDto>>
{
    /// <summary>
    /// Handles the AuditLogsQuery by retrieving a paginated collection of audit logs that are accessible to the current actor.
    /// </summary>
    /// <param name="query">The audit logs query containing pagination and filter criteria</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of audit log DTOs</returns>
    public async Task<IReadOnlyCollection<AuditLogDto>> HandleAsync(AuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        logger.AuditLogManyQueryStarted(currentActor.Guid, currentActor.ActorType, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorResolver.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var items = await auditLogQueryRepository.GetManyInfoOrderedByOccurredAtAsync(
            query.WithPagination,
            actor.PartitionAccessGuids,
            query.ActorGuid,
            query.ActorType,
            query.EntityGuid,
            query.EntityType,
            query.PeriodStart,
            query.PeriodEnd,
            cancellationToken
        );

        logger.AuditLogManyQueryCompleted(currentActor.Guid, currentActor.ActorType, actor.PartitionAccessGuids.Count, items.Count);

        return items;
    }
}
