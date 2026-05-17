namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents the kind of article to create.</summary>
public enum ArticleCreateKind
{
    Article = 0,
    Variation = 1,
    Pack = 2,
    Kit = 3,
    Container = 4,
}

/// <summary>Represents an article pack entry used when creating a kit article.</summary>
public sealed record ArticleCreateKitPackRequest(
    Guid ArticleGuid,
    double Quantity);

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
    bool? IsActive = null,
    ArticleCreateKind Kind = ArticleCreateKind.Article,
    Guid? FromArticleGuid = null,
    double? Quantity = null,
    IReadOnlyCollection<ArticleCreateKitPackRequest>? KitPacks = null,
    MassInfo? ContainerMaxMass = null);
