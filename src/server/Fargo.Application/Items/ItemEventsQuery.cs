using Fargo.Application.Common;
using Fargo.Core.Items.Events;

namespace Fargo.Application.Items;

/// <summary>
/// Represents a query for retrieving item events with optional filtering
/// criteria.
/// </summary>
/// <param name="WithPagination">
/// The pagination configuration limiting the number of results.
/// </param>
/// <param name="ArticleGuid">
/// Optional filter restricting events to a specific event-tracked article.
/// </param>
/// <param name="EventType">
/// Optional filter restricting events to a specific kind of event.
/// </param>
/// <param name="PeriodStart">
/// Optional filter for events that occurred at or after this instant.
/// </param>
/// <param name="PeriodEnd">
/// Optional filter for events that occurred at or before this instant.
/// </param>
public sealed record ItemEventsQuery(
    Pagination WithPagination,
    Guid? ArticleGuid = null,
    ItemEventType? EventType = null,
    DateTimeOffset? PeriodStart = null,
    DateTimeOffset? PeriodEnd = null
) : IQuery<PagedResult<ItemEventDto>>;
