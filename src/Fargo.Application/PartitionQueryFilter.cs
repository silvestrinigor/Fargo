namespace Fargo.Application;

/// <summary>
/// 
/// </summary>
public static class PartitionQueryFilter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="actorPartitionGuids"></param>
    /// <param name="requestedPartitionGuids"></param>
    /// <param name="notChildOfAnyPartition"></param>
    /// <returns></returns>
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
