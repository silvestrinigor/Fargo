using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Audits;

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
