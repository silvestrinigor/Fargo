using Fargo.Application.Persistence.Repositories;
using Fargo.Application.Common;
using Fargo.Application.Operations.Audits.Dtos;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.Audits;

/// <summary>
/// Represents a query for retrieving command audit logs with optional filtering.
/// </summary>
/// <param name="WithPagination">The pagination configuration for limiting the number of results.</param>
/// <param name="ActorGuid">Optional filter for audit logs by the acting actor's unique identifier.</param>
/// <param name="ActorType">Optional filter for audit logs by the acting actor's type.</param>
/// <param name="PeriodStart">Optional filter for audit logs starting from a specific date and time.</param>
/// <param name="PeriodEnd">Optional filter for audit logs ending at a specific date and time.</param>
public sealed record AuditLogsQuery(
    Pagination WithPagination,
    Guid? ActorGuid = null,
    ActorType? ActorType = null,
    DateTimeOffset? PeriodStart = null,
    DateTimeOffset? PeriodEnd = null
) : IQuery<PagedResult<AuditLogDto>>;

/// <summary>
/// Handles queries that retrieve a paginated collection of command audit logs
/// accessible to the current actor.
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
    /// Handles the query by retrieving the audit logs of actors the current actor
    /// can access.
    /// </summary>
    public async Task<PagedResult<AuditLogDto>> HandleAsync(AuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorResolver.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var accessScope = actor.ToAuditLogAccessScope();

        var result = await auditLogQueryRepository.GetManyInfoOrderedByOccurredAtAsync(
            query.WithPagination,
            accessScope,
            query.ActorGuid,
            query.ActorType,
            query.PeriodStart,
            query.PeriodEnd,
            cancellationToken
        );

        return result;
    }
}

/// <summary>
/// Builds the <see cref="AuditLogAccessScope"/> for a resolved principal.
/// </summary>
internal static class AuditLogAccessScopeExtensions
{
    public static AuditLogAccessScope ToAuditLogAccessScope(this SecurityPrincipal actor)
        => new(
            actor.Guid,
            [.. actor.PartitionAccessGuids],
            actor.HasPartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid));
}
