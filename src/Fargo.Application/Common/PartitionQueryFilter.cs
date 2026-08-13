namespace Fargo.Application.Common;

public static class PartitionQueryFilter
{
    public static IReadOnlyCollection<Guid>?
        ForPartitionedEntities(
            IReadOnlyCollection<Guid> actorPartitionGuids,
            IReadOnlyCollection<Guid>? requestedPartitionGuids)
    {
        if (requestedPartitionGuids is { Count: > 0 })
        {
            return (
                [.. actorPartitionGuids.Intersect(requestedPartitionGuids)]);
        }

        return actorPartitionGuids;
    }
}
