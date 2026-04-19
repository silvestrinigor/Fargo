namespace Fargo.Domain.Items;

/// <summary>
/// Represents a lightweight information projection of an Item entity.
/// </summary>
/// <remarks>
/// This value object contains only the minimal information required to reference
/// an item without loading the full aggregate. It is typically used in projections,
/// listings, or nested references inside other domain models.
/// </remarks>
/// <param name="Guid">
/// The unique identifier of the item.
/// </param>
/// <param name="ArticleGuid">
/// The unique identifier of the article to which the item belongs.
/// </param>
public sealed record ItemInformation(
    Guid Guid,
    Guid ArticleGuid
);
