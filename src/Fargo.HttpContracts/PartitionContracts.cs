namespace Fargo.HttpContracts;

public sealed record PartitionDto(
    Guid Guid,
    string Name,
    string Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid,
    PartitionModifiedType ModificationTypes
);

public sealed record PartitionCreateRequest(
    string Name,
    string? Description = null,
    Guid? ParentPartitionGuid = null
);

public sealed record PartitionUpdateRequest(
    string? Name = null,
    string? Description = null,
    Guid? ParentPartitionGuid = null,
    bool? IsActive = null
);
