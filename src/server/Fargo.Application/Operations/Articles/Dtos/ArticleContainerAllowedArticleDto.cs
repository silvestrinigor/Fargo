using UnitsNet;

namespace Fargo.Application.Operations.Articles.Dtos;

/// <summary>
/// Represents an article allowed inside a container, optionally constrained by
/// the maximum quantity of that article the container may hold.
/// </summary>
/// <param name="ArticleGuid">
/// The unique identifier of the article allowed inside the container.
/// </param>
/// <param name="MaxQuantity">
/// The optional maximum quantity of the allowed article the container may hold.
/// When <see langword="null"/>, no quantity cap applies. A maximum quantity may
/// only be supplied for an entity-tracked allowed article.
/// </param>
public sealed record ArticleContainerAllowedArticleDto(
    Guid ArticleGuid,
    Scalar? MaxQuantity = null
);
