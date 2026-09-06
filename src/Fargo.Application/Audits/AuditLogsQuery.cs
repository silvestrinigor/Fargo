using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Entities;

namespace Fargo.Application.Audits;

/// <summary>
/// Represents a query for retrieving audit logs with optional filtering criteria.
/// </summary>
/// <param name="WithPagination">The pagination configuration for limiting the number of results</param>
/// <param name="ActorGuid">Optional filter for audit logs by specific actor's unique identifier</param>
/// <param name="ActorType">Optional filter for audit logs by specific actor type</param>
/// <param name="EntityGuid">Optional filter for audit logs by specific entity's unique identifier</param>
/// <param name="EntityType">Optional filter for audit logs by specific entity type</param>
/// <param name="PeriodStart">Optional filter for audit logs starting from a specific date and time</param>
/// <param name="PeriodEnd">Optional filter for audit logs ending at a specific date and time</param>
public sealed record AuditLogsQuery(
    Pagination WithPagination,
    Guid? ActorGuid = null,
    ActorType? ActorType = null,
    Guid? EntityGuid = null,
    EntityType? EntityType = null,
    DateTimeOffset? PeriodStart = null,
    DateTimeOffset? PeriodEnd = null
) : IQuery<IReadOnlyCollection<AuditLogDto>>;
