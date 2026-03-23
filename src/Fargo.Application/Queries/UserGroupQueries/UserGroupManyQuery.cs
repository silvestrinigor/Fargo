using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserGroupQueries;

/// <summary>
/// Query used to retrieve a paginated collection of user group information
/// accessible to the current user.
/// </summary>
/// <param name="UserGuid">
/// Optional filter used to retrieve only user groups associated with a specific user.
/// When provided, only groups assigned to the specified user are returned.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned results represent the state of the user groups
/// as they existed at the specified date and time.
/// </param>
/// <param name="Pagination">
/// Optional pagination configuration used to control the number of returned
/// results and the starting position of the query.
/// When <see langword="null"/>, a default pagination is used.
/// </param>
public sealed record UserGroupManyQuery(
    Guid? UserGuid = null,
    DateTimeOffset? AsOfDateTime = null,
    Pagination? Pagination = null
) : IQuery<IReadOnlyCollection<UserGroupInformation>>;

/// <summary>
/// Handles the execution of <see cref="UserGroupManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// only user groups that belong to at least one of those partitions.
///
/// If the current user has no accessible partitions, the repository returns
/// an empty result set.
/// </remarks>
public sealed class UserGroupManyQueryHandler(
        ActorService actorService,
    IUserGroupRepository userGroupRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupInformation>>
{
    /// <summary>
    /// Executes the query to retrieve user group information accessible
    /// to the current user.
    /// </summary>
    /// <param name="query">
    /// The query containing the optional user filter, as-of date,
    /// and pagination settings.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="UserGroupInformation"/> objects
    /// accessible to the current user.
    /// </returns>
    public async Task<IReadOnlyCollection<UserGroupInformation>> Handle(
        UserGroupManyQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            var userGroups = await userGroupRepository.GetManyInfo(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    query.UserGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return userGroups;
        }
        else
        {
            var userGroups = await userGroupRepository.GetManyInfoInPartitions(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    actor.PartitionAccesses,
                    query.UserGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return userGroups;
        }
    }
}
