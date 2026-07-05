namespace Fargo.Application.Shared.Items;

public sealed record ItemCreateDto(
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null);