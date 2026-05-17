namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents the article payload inside an article create request.</summary>
public sealed record ArticleCreateRequest(
    string Name,
    string? Description = null,
    string? Color = null,
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
    bool? IsActive = null);
