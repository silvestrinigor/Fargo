using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserQueries;

/// <summary>
/// Query used to retrieve a single user information projection
/// accessible to the current user.
/// </summary>
/// <param name="UserGuid">
/// The unique identifier of the user to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the user
/// as it existed at the specified date and time.
/// </param>
public sealed record UserSingleQuery(
    Guid UserGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserInformation?>;

/// <summary>
/// Handles the execution of <see cref="UserSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// the requested user only if it belongs to at least one of those partitions.
///
/// If the user does not exist or is not accessible to the current user,
/// <see langword="null"/> is returned.
/// </remarks>
public sealed class UserSingleQueryHandler(
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserSingleQuery, UserInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single user information projection
    /// accessible to the current user.
    /// </summary>
    public async Task<UserInformation?> Handle(
        UserSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        var partitionAccessGuids = await partitionRepository.GetDescendantGuids(
            [.. actor.PartitionAccesses.Select(x => x.PartitionGuid)],
            includeRoots: true,
            cancellationToken
        );

        var userInformation = await userRepository.GetInfoByGuidInPartitions(
            query.UserGuid,
            partitionAccessGuids,
            query.AsOfDateTime,
            cancellationToken
        );

        return userInformation;
    }
}
