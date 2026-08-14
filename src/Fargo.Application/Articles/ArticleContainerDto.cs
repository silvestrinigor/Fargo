using UnitsNet;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the container information associated with an article.
/// </summary>
/// <param name="MaxMass">
/// The optional maximum mass that the container can hold.
/// </param>
public sealed record ArticleContainerDto(
    Mass? MaxMass = null
);
