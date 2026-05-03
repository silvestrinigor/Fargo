namespace Fargo.Application.Articles;

/// <summary>
/// Data used to fully replace an existing article. PUT semantics — every field is required;
/// missing list elements (Partitions, Barcodes) are interpreted as "empty".
/// </summary>
/// <param name="Name">The new name of the article.</param>
/// <param name="Description">The new description, or null to clear it.</param>
/// <param name="Metrics">The new physical measurements, or null to leave unset.</param>
/// <param name="ShelfLife">The new shelf life, or null to leave unset.</param>
/// <param name="Partitions">The full set of partition guids the article should belong to.</param>
/// <param name="Barcodes">The full set of barcodes the article should have.</param>
/// <param name="IsActive">Whether the article is active.</param>
public sealed record ArticleUpdateModel(
    string Name,
    string? Description = null,
    ArticleMetricsModel? Metrics = null,
    TimeSpan? ShelfLife = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    ArticleBarcodesModel? Barcodes = null,
    bool IsActive = true);
