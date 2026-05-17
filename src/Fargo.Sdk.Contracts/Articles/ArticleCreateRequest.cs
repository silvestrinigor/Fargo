namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents an article pack entry used when creating a kit article.</summary>
public sealed record ArticleCreateKitPackRequest(
    Guid ArticleGuid,
    double Quantity);

/// <summary>Represents variation article creation details.</summary>
public sealed record ArticleCreateVariationRequest(
    Guid FromArticleGuid);

/// <summary>Represents pack article creation details.</summary>
public sealed record ArticleCreatePackRequest(
    Guid FromArticleGuid,
    double Quantity);

/// <summary>Represents kit article creation details.</summary>
public sealed record ArticleCreateKitRequest(
    IReadOnlyCollection<ArticleCreateKitPackRequest> Packs);

/// <summary>Represents container article creation details.</summary>
public sealed record ArticleCreateContainerRequest(
    MassInfo? MaxMass = null);

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
    ArticleCreateVariationRequest? Variation = null,
    ArticleCreatePackRequest? Pack = null,
    ArticleCreateKitRequest? Kit = null,
    ArticleCreateContainerRequest? Container = null);
