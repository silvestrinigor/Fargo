namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents the article payload inside an article create request.</summary>
public sealed record ArticleCreateDto(
    string Name,
    string? Description = null,
    ArticleMetricsDto? Metrics = null,
    TimeSpan? ShelfLife = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    ArticleBarcodesDto? Barcodes = null,
    bool? IsActive = null);
