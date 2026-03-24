using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserGroupQueries;

/// <summary>
/// Query used to retrieve a single <see cref="UserGroupInformation"/> projection
/// accessible to the current actor.
/// </summary>
/// <param name="UserGroupGuid">
/// The unique identifier of the user group to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the user group
/// as it existed at the specified date and time.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only user groups visible to the current actor are returned.
/// </remarks>
public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserGroupInformation?>;

/// <summary>
/// Handles <see cref="UserGroupSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Filtering the user group based on partition access.</description></item>
/// <item><description>Applying optional temporal (as-of) constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges bypass partition filtering
/// and can access any user group.
/// </para>
///
/// <para>
/// Regular actors can only access the user group if it belongs to at least one
/// partition they have access to.
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if the user group belonged
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
/// Regular actors will not have access to the user group, as the partition
/// no longer exists in the current context, and <see langword="null"/> is returned.
/// </description>
/// </item>
/// </list>
/// </para>
///
/// <para>
/// If the user group does not exist or is not accessible to the current actor,
/// <see langword="null"/> is returned.
/// </para>
/// </remarks>
public sealed class UserGroupSingleQueryHandler(
        ActorService actorService,
    IUserGroupRepository userGroupRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserGroupSingleQuery, UserGroupInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single user group information projection
    /// accessible to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing the user group identifier and optional temporal parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="UserGroupInformation"/> visible to the current actor,
    /// or <see langword="null"/> if the user group does not exist or is not accessible.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// If the actor has administrative or system privileges, the user group is retrieved
    /// without partition filtering.
    ///
    /// Otherwise, the user group is only returned if it belongs to at least one
    /// partition accessible to the actor.
    ///
    /// This method follows the query-side soft-authorization pattern and does not
    /// throw when access is denied.
    /// </remarks>
    public async Task<UserGroupInformation?> Handle(
        UserGroupSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            var userGroup = await userGroupRepository.GetInfoByGuid(
                    query.UserGroupGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return userGroup;
        }
        else
        {
            var userGroup = await userGroupRepository.GetInfoByGuidInPartitions(
                    query.UserGroupGuid,
                    actor.PartitionAccesses,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return userGroup;
        }
    }
}
