namespace Fargo.Core.Entities.Partitions;

/// <summary>
/// An object scoped to one or more partitions.
/// </summary>
public interface IPartitionedGuidsReadOnly
{
    IReadOnlyCollection<Guid> PartitionGuids { get; }
}
