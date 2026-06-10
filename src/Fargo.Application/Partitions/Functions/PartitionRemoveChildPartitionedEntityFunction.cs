using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Fargo.Core.Actors;

namespace Fargo.Application.Partitions.Functions;

internal static class PartitionRemoveChildPartitionedEntityFunction
{
    internal static void PartitionedRemoveFromPartition(IPartitionedEntity partitioned, Partition partition, Actor actor, IPartitionEventRepository partitionEventRepository)
    {
        partitioned.RemovePartition(partition, actor);

        var removedFromPartitionEvent = PartitionEvent.RemovedFromPartition(
            partitioned,
            partition,
            actor.Guid);

        partitionEventRepository.Add(removedFromPartitionEvent);
    }
}
