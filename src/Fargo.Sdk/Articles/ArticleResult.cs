namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents an article returned by the API.
/// </summary>
/// <param name="Guid">The unique identifier of the article.</param>
/// <param name="Name">The name of the article.</param>
/// <param name="Description">A short description of the article.</param>
public sealed record ArticleResult(
    Guid Guid,
    string Name,
    string Description,
    MassDto? Mass = null
);
