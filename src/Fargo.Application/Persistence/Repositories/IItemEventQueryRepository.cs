using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.Items;
using Fargo.Core.Events;

namespace Fargo.Application.Persistence.Repositories;

public interface IItemEventQueryRepository
{
    /// <summary>
    /// Ordered by occurrence time. Null <c>childOfAnyOfThesePartitions</c> applies no partition filter and
    /// an empty collection matches nothing; <paramref name="periodStart"/> and
    /// <paramref name="periodEnd"/> are inclusive.
    /// </summary>
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
