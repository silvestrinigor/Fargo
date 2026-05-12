namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents an article update request (full replacement).</summary>
public sealed record ArticleUpdateRequest(
    string Name,
    string? Description = null,
    ArticleMetricsInfo? Metrics = null,
    TimeSpan? ShelfLife = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    string? Ean13 = null,
    string? Ean8 = null,
    string? UpcA = null,
    string? UpcE = null,
    string? Code128 = null,
    string? Code39 = null,
    string? Itf14 = null,
    string? Gs1128 = null,
    string? QrCode = null,
    string? DataMatrix = null,
    bool IsActive = true);
