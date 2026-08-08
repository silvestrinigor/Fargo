using Fargo.Core.Shared.Informations;

namespace Fargo.Application.Shared.Partitions;

public sealed record PartitionUpdateDto(
    Name? Name = null,
    Description? Description = null,
    Guid? ParentPartitionGuid = null
);
