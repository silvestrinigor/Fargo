using Fargo.Application.Shared.Partitions;

namespace Fargo.Application.Partitions;

public sealed record PartitionSingleQuery(
    Guid PartitionGuid
) : IQuery<PartitionDto?>;
