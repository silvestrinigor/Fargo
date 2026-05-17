namespace Fargo.Sdk.Contracts.Items;

/// <summary>Represents the item payload inside an item create request.</summary>
public sealed record ItemCreateRequest(
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null);
