using Fargo.Core.Actors;
using Fargo.Core.Events;
using Fargo.Core.Partitions;

namespace Fargo.Application.Partitions.Functions;

internal static class PartitionAddChildPartitionedEntityFunction
{
    internal static void PartitionedInsertIntoPartition(IPartitioned partitioned, Partition partition, Actor actor, IPartitionEventRepository partitionEventRepository)
    {
        partitioned.AddPartition(partition, actor);

        var insertedIntoPartitionEvent = PartitionEvent.InsertedIntoPartition(
            partitioned,
            partition,
            actor.Guid);

        partitionEventRepository.Add(insertedIntoPartitionEvent);
    }
}
