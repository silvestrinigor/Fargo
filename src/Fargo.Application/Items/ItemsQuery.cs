using Fargo.Application.Shared.Items;

namespace Fargo.Application.Items;

public sealed record ItemsQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<ItemDto>>;
