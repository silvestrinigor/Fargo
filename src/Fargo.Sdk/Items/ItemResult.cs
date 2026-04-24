namespace Fargo.Sdk.Items;

/// <summary>
/// Represents an item returned by the API.
/// </summary>
/// <param name="Guid">The unique identifier of the item.</param>
/// <param name="ArticleGuid">The unique identifier of the article this item is an instance of.</param>
public sealed record ItemResult(
    Guid Guid,
    Guid ArticleGuid,
    Guid? EditedByGuid = null
);
