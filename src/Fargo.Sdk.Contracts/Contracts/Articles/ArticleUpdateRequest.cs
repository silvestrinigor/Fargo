namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents an article update request (full replacement).</summary>
public sealed record ArticleUpdateRequest(
    string Name,
    string? Description = null,
    ArticleMetricsInfo? Metrics = null,
    TimeSpan? ShelfLife = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    ArticleBarcodesInfo? Barcodes = null,
    bool IsActive = true);
