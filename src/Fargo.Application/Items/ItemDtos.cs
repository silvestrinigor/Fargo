namespace Fargo.Application.Items;

public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate,
    IReadOnlyCollection<Guid> Partitions,
    Guid? EditedByGuid
);

public sealed record ItemCreateDto(
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    IReadOnlyCollection<Guid>? Partitions = null
);

public sealed record ItemUpdateDto(
    DateTimeOffset? ProductionDate,
    IReadOnlyCollection<Guid> Partitions
);
