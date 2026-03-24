using Fargo.Application.Models.TreeModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Repositories;

/// <summary>
/// Provides access methods for retrieving partition nodes in a hierarchical structure.
/// </summary>
public interface IPartitionTreeRepository
{
    /// <summary>
    /// Retrieves the child partitions of a given partition as tree nodes.
    /// </summary>
    /// <remarks>
    /// This method returns a paginated collection of <see cref="TreeNode"/> representing
    /// the direct children of the specified partition.
    ///
    /// If <paramref name="partitionGuid"/> is <c>null</c>, the method returns the root-level
    /// partitions.
    ///
    /// Each returned <see cref="TreeNode"/> will indicate whether it has child partitions
    /// through the <c>HasMembers</c> property.
    /// </remarks>
    /// <param name="pagination">
    /// The pagination parameters used to limit and control the result set.
    /// </param>
    /// <param name="partitionGuid">
    /// The unique identifier of the parent partition. If <c>null</c>, root partitions are returned.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="TreeNode"/> representing the child partitions.
    /// </returns>
    Task<IReadOnlyCollection<TreeNode>> GetPartitionChilds(
        Pagination pagination,
        Guid? partitionGuid,
        CancellationToken cancellationToken = default);
}
