using Fargo.Application.Common;
using Fargo.Application.Shared.Partitions;

namespace Fargo.Application.Partitions;

public sealed record PartitionsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<PartitionDto>>;
