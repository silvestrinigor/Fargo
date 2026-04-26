using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Lightweight projection of an <see cref="Article"/> used for read operations.
/// </summary>
/// <param name="Guid">The unique identifier of the article.</param>
/// <param name="Name">The name of the article.</param>
/// <param name="Description">The description of the article.</param>
/// <param name="Metrics">Physical measurements (mass and dimensions). May be <see langword="null"/> when no measurements have been set.</param>
/// <param name="ShelfLife">The shelf life of the article, or <see langword="null"/> if unset.</param>
/// <param name="HasImage">Whether the article has a stored image.</param>
/// <param name="EditedByGuid">The identifier of the user who last edited this article.</param>
public sealed record ArticleInformation(
    Guid Guid,
    Name Name,
    Description Description,
    ArticleMetrics? Metrics = null,
    TimeSpan? ShelfLife = null,
    bool HasImage = false,
    Guid? EditedByGuid = null
);
