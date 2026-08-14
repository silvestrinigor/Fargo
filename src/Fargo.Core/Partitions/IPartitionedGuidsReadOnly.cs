namespace Fargo.Core.Partitions;

/// <summary>
/// Defines a read-only contract for objects associated with one or more partitions.
/// </summary>
public interface IPartitionedGuidsReadOnly
{
    /// <summary>
    /// Gets the unique identifiers of the partitions associated with the object.
    /// </summary>
    IReadOnlyCollection<Guid> PartitionGuids { get; }
}
