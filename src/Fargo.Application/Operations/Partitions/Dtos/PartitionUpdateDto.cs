using Fargo.Core.Informations;

namespace Fargo.Application.Operations.Partitions.Dtos;

public sealed record PartitionUpdateDto(
    Name? Name = null,
    Description? Description = null,
    Guid? ParentPartitionGuid = null
);
