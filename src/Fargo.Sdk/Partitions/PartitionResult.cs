namespace Fargo.Sdk.Partitions;

/// <summary>
/// Represents a partition returned by the API.
/// </summary>
public sealed record PartitionResult(
    Guid Guid,
    string Name,
    string Description,
    Guid? ParentPartitionGuid,
    bool IsActive
);
