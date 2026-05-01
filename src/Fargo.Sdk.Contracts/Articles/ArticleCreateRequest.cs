namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents the article payload inside an article create request.</summary>
public sealed record ArticleCreateDto(
    string Name,
    string? Description = null,
    Guid? FirstPartition = null,
    ArticleMetricsDto? Metrics = null,
    TimeSpan? ShelfLife = null);

/// <summary>Represents an article create request.</summary>
public sealed record ArticleCreateRequest(ArticleCreateDto Article);

/// <summary>Represents an article update request.</summary>
public sealed record ArticleUpdateRequest(
    string? Name = null,
    string? Description = null,
    ArticleMetricsDto? Metrics = null,
    TimeSpan? ShelfLife = null);
