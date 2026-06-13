using Fargo.Core.Actors;
using Fargo.Core.Events;
using Fargo.Core.Partitions;

namespace Fargo.Application.Partitions.Functions;

internal static class PartitionRemoveChildPartitionedEntityFunction
{
    internal static void PartitionedRemoveFromPartition(IPartitioned partitioned, Partition partition, Actor actor, IPartitionEventRepository partitionEventRepository)
    {
        partitioned.RemovePartition(partition, actor);

        var removedFromPartitionEvent = PartitionEvent.RemovedFromPartition(
            partitioned,
            partition,
            actor.Guid);

        partitionEventRepository.Add(removedFromPartitionEvent);
    }
}
