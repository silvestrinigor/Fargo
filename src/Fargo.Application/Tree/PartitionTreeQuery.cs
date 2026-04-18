using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Tree;

/// <summary>
/// Query used to retrieve a paginated collection of partition tree nodes.
/// </summary>
/// <param name="ParentPartitionGuid">
/// The unique identifier of the parent partition.
/// If <c>null</c>, the root-level partitions will be retrieved.
/// </param>
/// <param name="IncludedTypes">
/// An optional collection of <see cref="TreeNodeType"/> values used to filter
/// which node types are included in the result.
/// If <c>null</c>, all node types are included.
/// </param>
/// <param name="Pagination">
/// The pagination settings used to limit and organize the result set.
/// If not provided, defaults to <see cref="Pagination.FirstPage20Items"/>.
/// </param>
public sealed record PartitionTreeQuery(
    Guid? ParentPartitionGuid = null,
    IReadOnlyCollection<TreeNodeType>? IncludedTypes = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<EntityTreeNode>>;

/// <summary>
/// Handles <see cref="PartitionTreeQuery"/> requests.
/// </summary>
/// <remarks>
/// This handler retrieves partition tree nodes from the repository and filters them
/// based on the current actor's access permissions.
/// </remarks>
public sealed class PartitionTreeQueryHandler(
    ActorService actorService,
    IPartitionTreeRepository partitionTreeRepository,
    ICurrentUser currentUser)
    : IQueryHandler<PartitionTreeQuery, IReadOnlyCollection<EntityTreeNode>>
{
    /// <summary>
    /// Handles the query and returns a filtered collection of partition tree nodes
    /// that the current actor has access to.
    /// </summary>
    /// <param name="query">
    /// The query containing filtering and pagination parameters.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="EntityTreeNode"/> that the actor is authorized to access.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="query"/> is <c>null</c>.
    /// </exception>
    public async Task<IReadOnlyCollection<EntityTreeNode>> Handle(
        PartitionTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var partitionsTreeNodes = await partitionTreeRepository.GetPartitionTreeNodes(
            query.Pagination ?? Pagination.FirstPage20Items,
            query.ParentPartitionGuid,
            query.IncludedTypes,
            cancellationToken
            );

        var partitionsTreeNodesThatActorHasAccess =
            partitionsTreeNodes.Where(p => actor.HasPartitionAccess(p.EntityGuid));

        return [.. partitionsTreeNodesThatActorHasAccess];
    }
}
