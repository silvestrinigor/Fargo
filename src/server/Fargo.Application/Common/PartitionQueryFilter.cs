namespace Fargo.Application.Common;

/// <summary>
/// Provides utility methods for filtering partition queries based on actor permissions and requested partitions.
/// This class helps ensure that actors can only access entities within their permitted partitions.
/// </summary>
public static class PartitionQueryFilter
{
    /// <summary>
    /// Filters partition GUIDs for partitioned entities based on actor permissions and requested partitions.
    /// If requested partition GUIDs are provided, the result will be the intersection of actor partition GUIDs and requested partition GUIDs.
    /// Otherwise, all actor partition GUIDs are returned.
    /// </summary>
    /// <param name="actorPartitionGuids">The collection of partition GUIDs that the actor is permitted to access</param>
    /// <param name="requestedPartitionGuids">Optional collection of partition GUIDs that were requested by the query</param>
    /// <returns>
    /// A collection of partition GUIDs that represent the intersection of actor permissions and requested partitions,
    /// or all actor partition GUIDs if no specific partitions were requested
    /// </returns>
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
