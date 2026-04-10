namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents an article returned by the API.
/// </summary>
public sealed record ArticleResult(
    Guid Guid,
    string Name,
    string Description
);
