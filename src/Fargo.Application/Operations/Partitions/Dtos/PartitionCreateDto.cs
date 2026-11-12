using Fargo.Core.Informations;

namespace Fargo.Application.Operations.Partitions.Dtos;

public sealed record PartitionCreateDto(
    Name Name,
    Guid? ParentPartitionGuid = null,
    Description? Description = null);
