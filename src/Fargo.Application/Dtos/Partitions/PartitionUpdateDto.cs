using Fargo.Core.Informations;

namespace Fargo.Application.Dtos.Partitions;

public sealed record PartitionUpdateDto(
    Name? Name = null,
    Description? Description = null
);
