namespace Fargo.Sdk.Contracts.Partitions;

/// <summary>Represents a partition create request.</summary>
public sealed record PartitionCreateRequest(
    string Name,
    string? Description = null,
    Guid? ParentPartitionGuid = null);

/// <summary>Represents a partition update request.</summary>
public sealed record PartitionUpdateRequest(
    string? Name = null,
    string? Description = null,
    Guid? ParentPartitionGuid = null,
    bool? IsActive = null);
