namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents the article payload inside an article create request.</summary>
public sealed record ArticleCreateRequest(
    string Name,
    string? Description = null,
    ArticleMetricsInfo? Metrics = null,
    TimeSpan? ShelfLife = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    ArticleBarcodesInfo? Barcodes = null,
    bool? IsActive = null);
