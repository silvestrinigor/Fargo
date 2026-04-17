using Fargo.Application.Models.TreeModels;
using Fargo.Domain;

namespace Fargo.Application.Repositories;

/// <summary>
/// Provides access methods for retrieving tree nodes within a partition-based hierarchy.
/// </summary>
/// <remarks>
/// This repository exposes a unified way to retrieve heterogeneous nodes
/// (e.g., partitions, users, user groups, articles, items) that belong to a partition.
///
/// The returned nodes represent a "membership tree", where partitions act as containers
/// and other entities are members of those partitions.
/// </remarks>
public interface IPartitionTreeRepository
{
    /// <summary>
    /// Retrieves a paginated collection of tree nodes associated with a partition.
    /// </summary>
    /// <remarks>
    /// When <paramref name="partitionGuid"/> is <c>null</c>, the method returns:
    /// <list type="bullet">
    /// <item><description>Root-level partitions (partitions without a parent)</description></item>
    /// <item><description>Entities that are not associated with any partition</description></item>
    /// </list>
    ///
    /// When <paramref name="partitionGuid"/> is provided, the method returns:
    /// <list type="bullet">
    /// <item><description>Child partitions of the specified partition</description></item>
    /// <item><description>Entities associated with the specified partition</description></item>
    /// </list>
    ///
    /// The result may include multiple node types depending on the <paramref name="includedTypes"/> filter.
    ///
    /// Each returned <see cref="EntityTreeNode"/> includes a flag indicating whether it has child nodes,
    /// which is primarily applicable to partitions in this context.
    /// </remarks>
    Task<IReadOnlyCollection<EntityTreeNode>> GetPartitionTreeNodes(
        Pagination pagination,
        Guid? partitionGuid,
        IReadOnlyCollection<TreeNodeType>? includedTypes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated collection of tree nodes associated with a partition,
    /// filtered by a set of accessible partitions.
    /// </summary>
    /// <remarks>
    /// This method applies the same behavior as <see cref="GetPartitionTreeNodes"/>,
    /// but restricts the result to nodes that belong to the specified <paramref name="partitionGuids"/>.
    ///
    /// When <paramref name="partitionGuid"/> is <c>null</c>, the method returns:
    /// <list type="bullet">
    /// <item><description>Root-level partitions within the accessible partitions</description></item>
    /// <item><description>Entities not associated with any partition or associated with the accessible partitions</description></item>
    /// </list>
    ///
    /// When <paramref name="partitionGuid"/> is provided, the method returns:
    /// <list type="bullet">
    /// <item><description>Child partitions of the specified partition, restricted to accessible partitions</description></item>
    /// <item><description>Entities associated with the specified partition and within the accessible partitions</description></item>
    /// </list>
    ///
    /// This method is typically used to enforce access control by limiting results
    /// to partitions that the current actor is allowed to access.
    /// </remarks>
    /// <param name="pagination">
    /// The pagination parameters used to limit and order the result set.
    /// </param>
    /// <param name="partitionGuids">
    /// The collection of partition identifiers used to restrict the result set.
    /// </param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition whose contents should be retrieved.
    /// If <c>null</c>, root-level partitions and unassigned entities are returned.
    /// </param>
    /// <param name="includedTypes">
    /// An optional collection of <see cref="TreeNodeType"/> values used to filter
    /// which types of nodes should be included in the result.
    /// If <c>null</c>, all supported node types are included.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="EntityTreeNode"/> representing the nodes
    /// filtered by the specified partitions.
    /// </returns>
    Task<IReadOnlyCollection<EntityTreeNode>> GetPartitionTreeNodesInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        Guid? partitionGuid,
        IReadOnlyCollection<TreeNodeType>? includedTypes = null,
        CancellationToken cancellationToken = default);
}
