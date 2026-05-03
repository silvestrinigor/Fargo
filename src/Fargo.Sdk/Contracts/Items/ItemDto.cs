namespace Fargo.Sdk.Contracts.Items;

/// <summary>Represents an item returned by the API.</summary>
public sealed record ItemDto(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    Guid? EditedByGuid = null);
