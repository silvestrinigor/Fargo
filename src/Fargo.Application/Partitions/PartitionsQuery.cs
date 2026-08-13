using Fargo.Application.Common;

namespace Fargo.Application.Partitions;

public sealed record PartitionsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null
) : IQuery<IReadOnlyCollection<PartitionDto>>;
