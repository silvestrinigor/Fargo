namespace Fargo.Application.Items;

/// <summary>
/// Represents the inventory of a single article contained in a container.
/// </summary>
/// <param name="ArticleGuid">The article the quantity belongs to.</param>
/// <param name="TotalCount">
/// The total quantity of the article in the container. For entity-tracked
/// articles this is a whole number of items; for event-tracked articles it is
/// the net quantity derived from movement events (received minus dispatched) and
/// may be fractional.
/// </param>
public sealed record ItemContainerInventoryDto(Guid ArticleGuid, double TotalCount);
