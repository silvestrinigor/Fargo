using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Users;

/// <summary>
/// Query used to retrieve a single <see cref="UserInformation"/> projection
/// accessible to the current actor.
/// </summary>
/// <param name="UserGuid">
/// The unique identifier of the user to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the user
/// as it existed at the specified date and time.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only users visible to the current actor are returned.
/// </remarks>
public sealed record UserSingleQuery(
    Guid UserGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserInformation?>;

/// <summary>
/// Handles <see cref="UserSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Filtering the user based on partition access.</description></item>
/// <item><description>Applying optional temporal (as-of) constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges bypass partition filtering
/// and can access any user.
/// </para>
///
/// <para>
/// Regular actors can access the user if it belongs to at least one
/// partition they have access to, or if the user has no partition (public).
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if the user belonged
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
/// Regular actors will not have access to the user, as the partition
/// no longer exists in the current context, and <see langword="null"/> is returned.
/// </description>
/// </item>
/// </list>
/// </para>
///
/// <para>
/// If the user does not exist or is not accessible to the current actor,
/// <see langword="null"/> is returned.
/// </para>
/// </remarks>
public sealed class UserSingleQueryHandler(
        ActorService actorService,
    IUserQueryRepository userRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserSingleQuery, UserInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single user information projection
    /// accessible to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing the user identifier and optional temporal parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="UserInformation"/> visible to the current actor,
    /// or <see langword="null"/> if the user does not exist or is not accessible.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// If the actor has administrative or system privileges, the user is retrieved
    /// without partition filtering.
    ///
    /// Otherwise, the user is only returned if it belongs to at least one
    /// partition accessible to the actor.
    ///
    /// This method follows the query-side soft-authorization pattern and does not
    /// throw when access is denied.
    /// </remarks>
    public async Task<UserInformation?> Handle(
        UserSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            var user = await userRepository.GetInfoByGuid(
                    query.UserGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return user;
        }
        else
        {
            var user = await userRepository.GetInfoByGuidPublicOrInPartitions(
                    query.UserGuid,
                    actor.PartitionAccessesGuids,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return user;
        }
    }
}
