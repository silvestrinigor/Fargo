namespace Fargo.Api.Items;

/// <summary>
/// Represents an item returned by the API.
/// </summary>
/// <param name="Guid">The unique identifier of the item.</param>
/// <param name="ArticleGuid">The unique identifier of the article this item is an instance of.</param>
/// <param name="ProductionDate">The production date of the item, or <see langword="null"/> if unknown.</param>
/// <param name="EditedByGuid">The identifier of the user who last edited this item.</param>
public sealed record ItemResult(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    Guid? EditedByGuid = null
);
