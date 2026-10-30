using Fargo.Application.Common;

namespace Fargo.Application.Partitions;

public sealed record PartitionsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<PartitionDto>>;
