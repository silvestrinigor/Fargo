namespace Fargo.Application.Items;

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate,
    Guid? ParentContainerGuid,
    IReadOnlyCollection<Guid> Partitions,
    Guid? EditedByGuid
);

public sealed record ItemCreateDto(
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    IReadOnlyCollection<Guid>? Partitions = null
);

public sealed record ItemUpdateDto(
    IReadOnlyCollection<Guid> Partitions,
    Guid? ParentContainerGuid = null
);
