using UnitsNet;

namespace Fargo.Core.Articles;

/// <summary>
/// Represents the information describing a single component of a kit article.
/// </summary>
/// <param name="FromArticleGuid">
/// The unique identifier of the article included as a component of the kit.
/// </param>
/// <param name="Quantity">
/// The quantity of the component article included in the kit.
/// </param>
public sealed record ArticleKitComponentInformation(
    Guid FromArticleGuid,
    Scalar Quantity
);
