namespace Fargo.Application.Articles;

public sealed record ArticleDto(
    Guid Guid,
    string Name,
    string Description,
    ArticleMetricsDto? Metrics,
    TimeSpan? ShelfLife,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
);
