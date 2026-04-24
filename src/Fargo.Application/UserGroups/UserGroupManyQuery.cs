using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Query used to retrieve a paginated collection of <see cref="UserGroupInformation"/>
/// accessible to the current actor.
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
/// When <see langword="null"/>, a default pagination is applied.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only user groups visible to the current actor are returned.
/// </remarks>
public sealed record UserGroupManyQuery(
    Guid? UserGuid = null,
    DateTimeOffset? AsOfDateTime = null,
    Pagination? Pagination = null
) : IQuery<IReadOnlyCollection<UserGroupInformation>>;

/// <summary>
/// Handles <see cref="UserGroupManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Filtering user groups based on partition access.</description></item>
/// <item><description>Applying optional user filter, temporal (as-of), and pagination constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges bypass partition filtering
/// and can access all user groups.
/// </para>
///
/// <para>
/// Regular actors can access user groups that belong to at least one
/// partition they have access to, or user groups with no partition (public).
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if a user group belonged
/// to a partition that has since been deleted at the time of the request,
/// the following rules apply:
/// <list type="bullet">
/// <item>
/// <description>
/// Administrative and system actors can still access the historical data.
/// </description>
/// </item>
/// <item>
/// <description>
/// Regular actors will not have access to such user groups, as the partition
/// no longer exists in the current context, and the result will be excluded
/// from the returned collection.
/// </description>
/// </item>
/// </list>
/// </para>
/// </remarks>
public sealed class UserGroupManyQueryHandler(
        ActorService actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupInformation>>
{
    /// <summary>
    /// Executes the query to retrieve user group information accessible
    /// to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing optional user filter, temporal parameter,
    /// and pagination settings.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="UserGroupInformation"/> objects
    /// visible to the current actor.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// If the actor has administrative or system privileges, user groups are retrieved
    /// without partition filtering.
    ///
    /// Otherwise, only user groups belonging to partitions accessible to the actor
    /// are returned.
    ///
    /// Pagination defaults to <see cref="Pagination.FirstPage20Items"/> when not specified.
    /// </remarks>
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
            var userGroups = await userGroupRepository.GetManyInfoInPartitionsOrPublic(
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
