namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents an article returned by the API.
/// </summary>
/// <param name="Guid">The unique identifier of the article.</param>
/// <param name="Name">The name of the article.</param>
/// <param name="Description">A short description of the article.</param>
/// <param name="Metrics">Physical measurements (mass and dimensions), or <see langword="null"/> if unset.</param>
/// <param name="ShelfLife">The shelf life of the article, or <see langword="null"/> if unset.</param>
/// <param name="HasImage">Whether the article has a stored image.</param>
/// <param name="EditedByGuid">The identifier of the user who last edited this article.</param>
public sealed record ArticleResult(
    Guid Guid,
    string Name,
    string Description,
    ArticleMetrics? Metrics = null,
    TimeSpan? ShelfLife = null,
    bool HasImage = false,
    Guid? EditedByGuid = null
);
