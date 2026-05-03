namespace Fargo.Application.Articles;

/// <summary>
/// Projection of an article used for read responses on the API and SDK.
/// </summary>
/// <param name="Guid">The unique identifier of the article.</param>
/// <param name="Name">The name of the article.</param>
/// <param name="Description">The description of the article.</param>
/// <param name="Metrics">Physical measurements (mass and dimensions). Null when unset.</param>
/// <param name="ShelfLife">The shelf life, or null when unset.</param>
/// <param name="Barcodes">The barcodes associated with the article.</param>
/// <param name="Partitions">Guids of partitions the article belongs to.</param>
/// <param name="IsActive">Whether the article is active.</param>
/// <param name="EditedByGuid">The user who last edited the article.</param>
public sealed record ArticleDto(
    Guid Guid,
    string Name,
    string Description,
    ArticleMetricsModel? Metrics,
    TimeSpan? ShelfLife,
    ArticleBarcodesModel Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid);
