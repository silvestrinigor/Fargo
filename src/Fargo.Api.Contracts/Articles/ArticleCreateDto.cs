namespace Fargo.Api.Contracts.Articles;

/// <summary>Represents the article payload inside an article create request.</summary>
public sealed record ArticleCreateDto(
    string Name,
    string? Description = null,
    Guid? FirstPartition = null,
    ArticleMetricsDto? Metrics = null,
    TimeSpan? ShelfLife = null);
