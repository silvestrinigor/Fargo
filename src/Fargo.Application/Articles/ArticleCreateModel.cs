namespace Fargo.Application.Articles;

/// <summary>
/// Data required to create a new article. Combines the article state and its initial
/// partition / barcode / activation set.
/// </summary>
/// <param name="Name">The name of the article.</param>
/// <param name="Description">Optional description.</param>
/// <param name="Metrics">Optional physical measurements.</param>
/// <param name="ShelfLife">Optional shelf life.</param>
/// <param name="Partitions">Partition guids the article should belong to. Defaults to empty.</param>
/// <param name="Barcodes">Initial barcodes for the article. Defaults to no barcodes.</param>
/// <param name="IsActive">Whether the article is active. Defaults to true.</param>
public sealed record ArticleCreateModel(
    string Name,
    string? Description = null,
    ArticleMetricsModel? Metrics = null,
    TimeSpan? ShelfLife = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    ArticleBarcodesModel? Barcodes = null,
    bool? IsActive = null);
