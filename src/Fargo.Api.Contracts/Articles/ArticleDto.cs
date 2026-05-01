namespace Fargo.Api.Contracts.Articles;

/// <summary>Represents an article returned by the API.</summary>
public sealed record ArticleDto(
    Guid Guid,
    string Name,
    string Description,
    ArticleMetricsDto? Metrics = null,
    TimeSpan? ShelfLife = null,
    bool HasImage = false,
    Guid? EditedByGuid = null,
    ArticleImagesDto? Images = null,
    ArticleBarcodesDto? Barcodes = null);
