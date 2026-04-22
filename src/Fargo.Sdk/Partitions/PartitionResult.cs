namespace Fargo.Sdk.Partitions;

/// <summary>
/// Represents a partition returned by the API.
/// </summary>
/// <param name="Guid">The unique identifier of the partition.</param>
/// <param name="Name">The display name of the partition.</param>
/// <param name="Description">A short description of the partition.</param>
/// <param name="ParentPartitionGuid">
/// The unique identifier of the parent partition, or <see langword="null"/> for
/// top-level partitions.
/// </param>
/// <param name="IsActive">Whether the partition is currently active.</param>
public sealed record PartitionResult(
    Guid Guid,
    string Name,
    string Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid = null
);
