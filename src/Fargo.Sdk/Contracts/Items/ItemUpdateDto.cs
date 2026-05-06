namespace Fargo.Sdk.Contracts.Items;

/// <summary>Represents an item update request.</summary>
public sealed record ItemUpdateDto(
    IReadOnlyCollection<Guid> Partitions,
    Guid? ParentContainerGuid = null);
