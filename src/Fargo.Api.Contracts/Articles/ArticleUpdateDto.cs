namespace Fargo.Api.Contracts.Articles;

/// <summary>Represents an article update request.</summary>
public sealed record ArticleUpdateDto(
    string? Name = null,
    string? Description = null,
    ArticleMetricsDto? Metrics = null,
    TimeSpan? ShelfLife = null);
