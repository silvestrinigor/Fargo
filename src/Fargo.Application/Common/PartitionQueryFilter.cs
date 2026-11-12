namespace Fargo.Application.Common;

public static class PartitionQueryFilter
{
    /// <summary>
    /// Narrows the partitions a query covers: the actor's partitions, intersected with the requested
    /// ones. A null or empty request means all of the actor's partitions.
    /// </summary>
    public static IReadOnlyCollection<Guid>?
        ForPartitionedEntities(IReadOnlyCollection<Guid> actorPartitionGuids, IReadOnlyCollection<Guid>? requestedPartitionGuids)
    {
        if (requestedPartitionGuids is { Count: > 0 })
        {
            return [.. actorPartitionGuids.Intersect(requestedPartitionGuids)];
        }

        return actorPartitionGuids;
    }
}
