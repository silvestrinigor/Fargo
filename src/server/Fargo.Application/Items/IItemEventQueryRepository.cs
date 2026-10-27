using Fargo.Application.Common;
using Fargo.Core.Items.Events;

namespace Fargo.Application.Items;

/// <summary>
/// Provides query operations for retrieving item event information.
/// </summary>
public interface IItemEventQueryRepository
{
    /// <summary>
    /// Retrieves a paginated collection of item events ordered by occurrence
    /// time, optionally filtered by article, event type, and time period.
    /// </summary>
    /// <param name="pagination">
    /// Pagination configuration limiting the number of results.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// When specified, only events whose article is accessible through one of
    /// these partitions are considered.
    /// </param>
    /// <param name="articleGuid">
    /// Optional filter restricting events to a specific article.
    /// </param>
    /// <param name="eventType">
    /// Optional filter restricting events to a specific kind of event.
    /// </param>
    /// <param name="periodStart">
    /// Optional filter for events that occurred at or after this instant.
    /// </param>
    /// <param name="periodEnd">
    /// Optional filter for events that occurred at or before this instant.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A page of item event DTOs ordered by occurrence time, together with the
    /// total number of matching entries.
    /// </returns>
    Task<PagedResult<ItemEventDto>> GetManyInfoOrderedByOccuredAtAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Guid? articleGuid = null,
        ItemEventType? eventType = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default
    );
}
