namespace Fargo.Domain.Partitions;

public interface IPartitioned
{
    IReadOnlyCollection<Guid> PartitionGuids { get; }
}
