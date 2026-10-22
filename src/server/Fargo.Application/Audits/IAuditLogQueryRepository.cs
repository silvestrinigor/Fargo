using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Entities;

namespace Fargo.Application.Audits;

/// <summary>
/// Provides query operations for retrieving audit log information.
/// </summary>
public interface IAuditLogQueryRepository
{
    /// <summary>
    /// Retrieves a single audit log by its unique identifier.
    /// </summary>
    /// <param name="auditLogGuid">The unique identifier of the audit log.</param>
    /// <param name="childOfAnyOfThesePartitions">
    /// An optional collection of partition identifiers. When specified, only
    /// audit logs accessible through one of these partitions are considered.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// the audit log DTO if a matching audit log is found; otherwise, <see langword="null"/>.</returns>
    Task<AuditLogDto?> GetInfoByGuidAsync(
        Guid auditLogGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves multiple audit log entries ordered by occurrence time.
    /// </summary>
    /// <param name="pagination">Pagination configuration for limiting the number of results</param>
    /// <param name="childOfAnyOfThesePartitions">Filters audit logs within the specified partitions</param>
    /// <param name="actorGuid">Optional filter for audit logs by specific actor's unique identifier</param>
    /// <param name="actorType">Optional filter for audit logs by specific actor type</param>
    /// <param name="entityGuid">Optional filter for audit logs by specific entity's unique identifier</param>
    /// <param name="entityType">Optional filter for audit logs by specific entity type</param>
    /// <param name="periodStart">Optional filter for audit logs starting from a specific date and time</param>
    /// <param name="periodEnd">Optional filter for audit logs ending at a specific date and time</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains
    /// a page of audit log DTOs ordered by occurrence time, together with the total
    /// number of matching entries.</returns>
    Task<PagedResult<AuditLogDto>> GetManyInfoOrderedByOccurredAtAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Guid? actorGuid = null,
        ActorType? actorType = null,
        Guid? entityGuid = null,
        EntityType? entityType = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default
    );
}
