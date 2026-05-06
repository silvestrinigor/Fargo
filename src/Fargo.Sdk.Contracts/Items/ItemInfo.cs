namespace Fargo.Sdk.Contracts.Items;

/// <summary>Represents an item returned by the API.</summary>
public sealed record ItemInfo(
    Guid Guid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    Guid? ParentContainerGuid = null,
    IReadOnlyCollection<Guid> Partitions = null!,
    Guid? EditedByGuid = null);
