using Fargo.Application.Common;
using Fargo.Application.Shared.Items;

namespace Fargo.Application.Items;

public sealed record ItemsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null
) : IQuery<IReadOnlyCollection<ItemDto>>;
