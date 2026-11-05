using UnitsNet;

namespace Fargo.Application.Operations.Articles.Dtos;

/// <summary>
/// Represents the container information associated with an article.
/// </summary>
/// <param name="MaxMass">
/// The optional maximum mass that the container can hold.
/// </param>
/// <param name="AllowedArticles">
/// The optional list of articles allowed inside the container, each paired with
/// the maximum quantity the container may hold. When omitted or empty, the
/// container imposes no allow-list constraint.
/// </param>
public sealed record ArticleContainerDto(
    Mass? MaxMass = null,
    IReadOnlyCollection<ArticleContainerAllowedArticleDto>? AllowedArticles = null
);
