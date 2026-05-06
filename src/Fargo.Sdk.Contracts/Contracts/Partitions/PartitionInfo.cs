namespace Fargo.Sdk.Contracts.Partitions;

/// <summary>Represents a partition returned by the API.</summary>
public sealed record PartitionInfo(
    Guid Guid,
    string Name,
    string Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid = null);
