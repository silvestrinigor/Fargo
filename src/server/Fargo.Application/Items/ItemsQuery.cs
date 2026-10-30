using Fargo.Application.Common;

namespace Fargo.Application.Items;

public sealed record ItemsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<ItemDto>>;
