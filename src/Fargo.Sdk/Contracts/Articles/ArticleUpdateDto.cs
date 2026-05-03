namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents an article update request (full replacement).</summary>
public sealed record ArticleUpdateDto(
    string Name,
    string? Description = null,
    ArticleMetricsDto? Metrics = null,
    TimeSpan? ShelfLife = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    ArticleBarcodesDto? Barcodes = null,
    bool IsActive = true);
