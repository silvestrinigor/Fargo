namespace Fargo.Core.Partitions;

public interface IPartitionedGuidsReadOnly
{
    IReadOnlyCollection<Guid> PartitionGuids { get; }
}
