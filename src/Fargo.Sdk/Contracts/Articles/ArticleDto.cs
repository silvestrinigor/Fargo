namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents an article returned by the API.</summary>
public sealed record ArticleDto(
    Guid Guid,
    string Name,
    string Description,
    ArticleMetricsDto? Metrics,
    TimeSpan? ShelfLife,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid);
