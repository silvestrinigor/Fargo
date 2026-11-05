using Fargo.Core.Informations;

namespace Fargo.Application.Operations.Partitions.Dtos;

public sealed record PartitionDto(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool? IsGlobalPartition
);
