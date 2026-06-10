using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Fargo.Core.Actors;

namespace Fargo.Application.Partitions.Functions;

internal static class PartitionAddChildPartitionedEntityFunction
{
    internal static void PartitionedInsertIntoPartition(IPartitionedEntity partitioned, Partition partition, Actor actor, IPartitionEventRepository partitionEventRepository)
    {
        partitioned.AddPartition(partition, actor);

        var insertedIntoPartitionEvent = PartitionEvent.InsertedIntoPartition(
            partitioned,
            partition,
            actor.Guid);

        partitionEventRepository.Add(insertedIntoPartitionEvent);
    }
}
