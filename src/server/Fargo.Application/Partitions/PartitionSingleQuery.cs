using Fargo.Application.Common;

namespace Fargo.Application.Partitions;

public sealed record PartitionSingleQuery(
    Guid PartitionGuid
) : IQuery<PartitionDto?>;
