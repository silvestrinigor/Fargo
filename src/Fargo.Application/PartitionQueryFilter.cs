namespace Fargo.Application;

public static class PartitionQueryFilter
{
    public static (
        IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions,
        bool? NotChildOfAnyPartition) ForPartitionedEntities(
            IReadOnlyCollection<Guid> actorPartitionGuids,
            IReadOnlyCollection<Guid>? requestedPartitionGuids,
            bool? notChildOfAnyPartition)
    {
        if (requestedPartitionGuids is { Count: > 0 })
        {
            return (
                [.. actorPartitionGuids.Intersect(requestedPartitionGuids)],
                notChildOfAnyPartition);
        }

        if (notChildOfAnyPartition is true)
        {
            return (null, true);
        }

        return (actorPartitionGuids, notChildOfAnyPartition ?? true);
    }
}
