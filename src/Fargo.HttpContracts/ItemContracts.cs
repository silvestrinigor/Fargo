namespace Fargo.HttpContracts;

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate,
    Guid? ParentContainerGuid,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid,
    ItemModifiedType ModificationTypes
);

public sealed record ItemCreateRequest(
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
);

public sealed record ItemUpdateRequest(
    IReadOnlyCollection<Guid> Partitions,
    Guid? ParentContainerGuid = null,
    bool? IsActive = null
);
