using UnitsNet;

namespace Fargo.Application.Operations.Articles.Dtos;

/// <summary>
/// Represents the packaging information for an article pack.
/// </summary>
/// <param name="FromArticleGuid">
/// The unique identifier of the article from which the pack is composed.
/// </param>
/// <param name="Quantity">
/// The quantity of the source article contained in the pack.
/// </param>
public sealed record ArticlePackDto(
    Guid FromArticleGuid,
    Scalar Quantity
);
