using Fargo.Application.Extensions;
using Fargo.Application.Models.TreeModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;
using Fargo.Domain;

namespace Fargo.Application.Queries.TreeQueries;

/// <summary>
/// Query used to retrieve a paginated collection of user tree nodes,
/// optionally scoped to a specific user group.
/// </summary>
/// <param name="UserGroupGuid">
/// The unique identifier of the user group to scope the results to.
/// If <c>null</c>, all accessible users are returned.
/// </param>
/// <param name="Pagination">
/// The pagination settings used to limit and organize the result set.
/// If not provided, defaults to <see cref="Pagination.FirstPage20Items"/>.
/// </param>
public sealed record UserTreeQuery(
    Guid? UserGroupGuid = null,
    Pagination? Pagination = null)
    : IQuery<IReadOnlyCollection<EntityTreeNode>>;

/// <summary>
/// Handles <see cref="UserTreeQuery"/> requests.
/// </summary>
/// <remarks>
/// Admins and system actors receive all user tree nodes.
/// Regular actors only receive nodes for users located in their accessible partitions.
/// </remarks>
public sealed class UserTreeQueryHandler(
    ActorService actorService,
    IUserTreeRepository userGroupTreeRepository,
    ICurrentUser currentUser)
    : IQueryHandler<UserTreeQuery, IReadOnlyCollection<EntityTreeNode>>
{
    public async Task<IReadOnlyCollection<EntityTreeNode>> Handle(
        UserTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            return await userGroupTreeRepository.GetUserTreeNodes(
                query.Pagination ?? Pagination.FirstPage20Items,
                query.UserGroupGuid,
                cancellationToken);
        }

        return await userGroupTreeRepository.GetUserTreeNodesInPartitions(
            query.Pagination ?? Pagination.FirstPage20Items,
            actor.PartitionAccesses,
            query.UserGroupGuid,
            cancellationToken);
    }
}
