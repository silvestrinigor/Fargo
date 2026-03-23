using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserGroupQueries;

/// <summary>
/// Query used to retrieve a single user group information projection
/// accessible to the current user.
/// </summary>
/// <param name="UserGroupGuid">
/// The unique identifier of the user group to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the user group
/// as it existed at the specified date and time.
/// </param>
public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserGroupInformation?>;

/// <summary>
/// Handles the execution of <see cref="UserGroupSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// the requested user group only if it belongs to at least one of those partitions.
///
/// If the user group does not exist or is not accessible to the current user,
/// <see langword="null"/> is returned.
/// </remarks>
public sealed class UserGroupSingleQueryHandler(
        ActorService actorService,
    IUserGroupRepository userGroupRepository,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserGroupSingleQuery, UserGroupInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single user group information projection
    /// accessible to the current user.
    /// </summary>
    /// <param name="query">
    /// The query containing the user group identifier and optional as-of date.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="UserGroupInformation"/> accessible to the current user,
    /// or <see langword="null"/> if the user group does not exist or is not accessible.
    /// </returns>
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
