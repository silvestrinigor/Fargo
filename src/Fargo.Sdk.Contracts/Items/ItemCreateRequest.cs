namespace Fargo.Sdk.Contracts.Items;

/// <summary>Represents the item payload inside an item create request.</summary>
public sealed record ItemCreateDto(
    Guid ArticleGuid,
    Guid? FirstPartition = null,
    DateTimeOffset? ProductionDate = null);

/// <summary>Represents an item create request.</summary>
public sealed record ItemCreateRequest(ItemCreateDto Item);

/// <summary>Represents an item update request.</summary>
public sealed record ItemUpdateRequest(DateTimeOffset? ProductionDate = null);
