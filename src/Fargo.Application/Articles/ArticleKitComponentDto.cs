using UnitsNet;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents a component of a kit article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article used as a component.</param>
/// <param name="Quantity">The quantity of the article included in the kit.</param>
public sealed record ArticleKitComponentDto(
    Guid ArticleGuid,
    Scalar Quantity
);
