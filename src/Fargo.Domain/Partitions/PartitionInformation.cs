namespace Fargo.Domain.Partitions;

/// <summary>
/// Represents a lightweight information projection of a Partition entity.
/// </summary>
/// <remarks>
/// This value object contains only the essential data required to reference
/// a partition without loading the full aggregate. It is typically used in
/// projections, listings, or hierarchical structures where partition metadata
/// is needed.
/// </remarks>
/// <param name="Guid">
/// The unique identifier of the partition.
/// </param>
/// <param name="Name">
/// The name of the partition.
/// </param>
/// <param name="Description">
/// A short description that provides additional details about the partition.
/// </param>
/// <param name="ParentPartitionGuid">
/// The unique identifier of the parent partition, if this partition belongs
/// to another partition in a hierarchical structure. Otherwise, <c>null</c>.
/// </param>
/// <param name="IsActive">
/// Indicates whether the partition is currently active and available for use.
/// </param>
public sealed record PartitionInformation(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid = null
);
