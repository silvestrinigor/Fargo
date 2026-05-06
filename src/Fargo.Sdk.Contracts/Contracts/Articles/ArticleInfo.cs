namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents an article returned by the API.</summary>
public sealed record ArticleInfo(
    Guid Guid,
    string Name,
    string Description,
    ArticleMetricsInfo? Metrics,
    TimeSpan? ShelfLife,
    ArticleBarcodesInfo Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid);
