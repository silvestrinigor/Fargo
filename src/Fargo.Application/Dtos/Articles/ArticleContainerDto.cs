using UnitsNet;

namespace Fargo.Application.Dtos.Articles;

/// <param name="AllowedArticles">
/// Empty or <see langword="null"/> means the container has no allow-list constraint.
/// </param>
public sealed record ArticleContainerDto(
    Mass? MaxMass = null,
    IReadOnlyCollection<ArticleContainerAllowedArticleDto>? AllowedArticles = null
);
