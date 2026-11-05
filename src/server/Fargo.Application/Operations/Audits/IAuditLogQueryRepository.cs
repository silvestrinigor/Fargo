using Fargo.Application.Common;
using Fargo.Application.Operations.Audits.Dtos;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.Audits;

/// <summary>
/// Provides query operations for retrieving command audit log information.
/// </summary>
public interface IAuditLogQueryRepository
{
    /// <summary>
    /// Retrieves a single audit log by its unique identifier, if it is accessible
    /// to the reader described by <paramref name="accessScope"/>.
    /// </summary>
    /// <param name="auditLogGuid">The unique identifier of the audit log.</param>
    /// <param name="accessScope">Describes which actors' audits the reader may access.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// The audit log DTO if found and accessible; otherwise, <see langword="null"/>.
    /// </returns>
    Task<AuditLogDto?> GetInfoByGuidAsync(
        Guid auditLogGuid,
        AuditLogAccessScope accessScope,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a page of audit logs accessible to the reader described by
    /// <paramref name="accessScope"/>, ordered by occurrence time.
    /// </summary>
    /// <param name="pagination">Pagination configuration for limiting the number of results.</param>
    /// <param name="accessScope">Describes which actors' audits the reader may access.</param>
    /// <param name="actorGuid">Optional filter for audit logs by the acting actor's unique identifier.</param>
    /// <param name="actorType">Optional filter for audit logs by the acting actor's type.</param>
    /// <param name="periodStart">Optional filter for audit logs starting from a specific date and time.</param>
    /// <param name="periodEnd">Optional filter for audit logs ending at a specific date and time.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    /// A page of audit log DTOs ordered by occurrence time, with the total count.
    /// </returns>
    Task<PagedResult<AuditLogDto>> GetManyInfoOrderedByOccurredAtAsync(
        Pagination pagination,
        AuditLogAccessScope accessScope,
        Guid? actorGuid = null,
        ActorType? actorType = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default
    );
}
