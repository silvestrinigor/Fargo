using Fargo.Application.Models.TreeModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Repositories;

/// <summary>
/// Repository responsible for retrieving user group-related tree nodes.
/// </summary>
/// <remarks>
/// Provides methods to query the hierarchical structure of user groups.
/// Currently, a user group child node represents a user, but this may be extended
/// in the future to include other user groups, enabling nested group hierarchies.
/// </remarks>
public interface IUserTreeRepository
{
    /// <summary>
    /// Retrieves a paginated collection of child tree nodes for a given user group.
    /// </summary>
    /// <param name="pagination">
    /// The pagination settings used to limit and organize the result set.
    /// </param>
    /// <param name="userGroupGuid">
    /// The unique identifier of the parent user group.
    /// If <c>null</c>, root-level nodes are retrieved, which may include both
    /// user groups and users.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="EntityTreeNode"/> representing
    /// the children of the specified user group, or root-level nodes when no group is specified.
    /// </returns>
    /// <remarks>
    /// Currently, user groups contain only users as children.
    /// Users do not belong to groups and can appear at the root level.
    /// Future implementations may allow user groups to contain other user groups.
    /// </remarks>
    Task<IReadOnlyCollection<EntityTreeNode>> GetUserTreeNodes(
        Pagination pagination,
        Guid? userGroupGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated collection of child tree nodes for a given user group,
    /// filtered by accessible partitions.
    /// </summary>
    /// <param name="pagination">
    /// The pagination settings used to limit and organize the result set.
    /// </param>
    /// <param name="accessiblePartitionGuids">
    /// The collection of partition identifiers used to restrict the result set
    /// based on access permissions.
    /// </param>
    /// <param name="userGroupGuid">
    /// The unique identifier of the parent user group.
    /// If <c>null</c>, root-level nodes are retrieved, which may include both
    /// user groups and users.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="EntityTreeNode"/> representing
    /// the children of the specified user group within accessible partitions,
    /// or root-level nodes when no group is specified.
    /// </returns>
    /// <remarks>
    /// Currently, user groups contain only users as children.
    /// Users do not belong to groups and can appear at the root level.
    /// Future implementations may allow user groups to contain other user groups.
    /// </remarks>
    Task<IReadOnlyCollection<EntityTreeNode>> GetUserTreeNodesInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> accessiblePartitionGuids,
        Guid? userGroupGuid,
        CancellationToken cancellationToken = default);
}
