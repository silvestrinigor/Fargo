using Fargo.Core.Entities;

namespace Fargo.Core.Partitions;

public interface IPartitionedGuids : IEntity
{
    IReadOnlyCollection<Guid> PartitionGuids { get; }
}
