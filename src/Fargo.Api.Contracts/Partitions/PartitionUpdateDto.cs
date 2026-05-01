namespace Fargo.Api.Contracts.Partitions;

/// <summary>Represents a partition update request.</summary>
public sealed record PartitionUpdateDto(
    string? Name = null,
    string? Description = null,
    Guid? ParentPartitionGuid = null,
    bool? IsActive = null);
