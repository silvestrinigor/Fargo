using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.Audits;

/// <summary>
/// Handles queries that retrieve a paginated collection of audit logs accessible
/// to the current actor.
/// </summary>
/// <param name="actorResolver">Resolves the current actor and its partition access.</param>
/// <param name="auditLogQueryRepository">Provides access to audit log query data.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
public sealed class AuditLogsQueryHandler(
    ActorResolver actorResolver,
    IAuditLogQueryRepository auditLogQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<AuditLogsQuery, PagedResult<AuditLogDto>>
{
    /// <summary>
    /// Handles the AuditLogsQuery by retrieving a paginated collection of audit logs that are accessible to the current actor.
    /// </summary>
    /// <param name="query">The audit logs query containing pagination and filter criteria</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a page of audit log DTOs together with the total count</returns>
    public async Task<PagedResult<AuditLogDto>> HandleAsync(AuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorResolver.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var result = await auditLogQueryRepository.GetManyInfoOrderedByOccurredAtAsync(
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

        return result;
    }
}
