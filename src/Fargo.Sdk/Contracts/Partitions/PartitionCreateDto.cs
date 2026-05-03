namespace Fargo.Sdk.Contracts.Partitions;

/// <summary>Represents a partition create request.</summary>
public sealed record PartitionCreateDto(
    string Name,
    string? Description = null,
    Guid? ParentPartitionGuid = null);
