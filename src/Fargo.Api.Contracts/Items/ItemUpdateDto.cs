namespace Fargo.Api.Contracts.Items;

/// <summary>Represents an item update request.</summary>
public sealed record ItemUpdateDto(DateTimeOffset? ProductionDate = null);
