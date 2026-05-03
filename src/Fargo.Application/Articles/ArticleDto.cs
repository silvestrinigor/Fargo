using Fargo.Domain;

namespace Fargo.Application.Articles;

public sealed record ArticleDto(
    Guid Guid,
    Name Name,
    Description Description,
    TimeSpan? ShelfLife,
    ArticleMetricsDto? Metrics,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
);
