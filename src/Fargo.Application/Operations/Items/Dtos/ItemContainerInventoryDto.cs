namespace Fargo.Application.Operations.Items.Dtos;

/// <param name="TotalCount">
/// Whole number for entity-tracked articles; for event-tracked articles the net of movement
/// events (received minus dispatched), which may be fractional.
/// </param>
public sealed record ItemContainerInventoryDto(Guid ArticleGuid, double TotalCount);
