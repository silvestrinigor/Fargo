namespace Fargo.Sdk.Items;

/// <summary>
/// Represents an item returned by the API.
/// </summary>
public sealed record ItemResult(
    Guid Guid,
    Guid ArticleGuid
);
