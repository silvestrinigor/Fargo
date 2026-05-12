namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents an article returned by the API.</summary>
public sealed record ArticleInfo(
    Guid Guid,
    string Name,
    string Description,
    ArticleMetricsInfo? Metrics,
    TimeSpan? ShelfLife,
    string? Ean13,
    string? Ean8,
    string? UpcA,
    string? UpcE,
    string? Code128,
    string? Code39,
    string? Itf14,
    string? Gs1128,
    string? QrCode,
    string? DataMatrix,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid);
