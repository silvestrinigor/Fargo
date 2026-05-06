namespace Fargo.Sdk.Contracts.Items;

/// <summary>Represents an item update request.</summary>
public sealed record ItemUpdateRequest(
    IReadOnlyCollection<Guid> Partitions,
    Guid? ParentContainerGuid = null);
