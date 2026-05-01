namespace Fargo.Api.Contracts.Partitions;

/// <summary>Represents a partition returned by the API.</summary>
public sealed record PartitionDto(
    Guid Guid,
    string Name,
    string Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid = null);
