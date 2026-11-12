using UnitsNet;

namespace Fargo.Application.Dtos.Articles;

/// <param name="MaxQuantity">
/// <see langword="null"/> means no cap. Only allowed for an entity-tracked allowed article.
/// </param>
public sealed record ArticleContainerAllowedArticleDto(
    Guid ArticleGuid,
    Scalar? MaxQuantity = null
);
