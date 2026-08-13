using Fargo.Core.Informations;

namespace Fargo.Application.Partitions;

public sealed record PartitionDto(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool? IsGlobalPartition
);
