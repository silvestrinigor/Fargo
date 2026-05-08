using Fargo.Domain;

namespace Fargo.Application.Partitions;

public sealed record PartitionDto(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record PartitionCreateDto(
    Name Name,
    Description? Description = null,
    Guid? ParentPartitionGuid = null
);

public sealed record PartitionUpdateDto(
    Name? Name = null,
    Description? Description = null,
    Guid? ParentPartitionGuid = null,
    bool? IsActive = null
);
