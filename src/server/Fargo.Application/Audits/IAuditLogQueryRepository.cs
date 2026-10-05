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
    /// the collection of audit log DTOs ordered by occurrence time</returns>
    Task<IReadOnlyCollection<AuditLogDto>> GetManyInfoOrderedByOccurredAtAsync(
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
