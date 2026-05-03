using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Domain;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Query used to retrieve the partitions that directly contain a specific user group.
/// </summary>
/// <param name="UserGroupGuid">The unique identifier of the user group.</param>
public sealed record UserGroupPartitionsQuery(Guid UserGroupGuid) : IQuery<IReadOnlyCollection<PartitionInformation>?>;

/// <summary>
/// Handles <see cref="UserGroupPartitionsQuery"/>.
/// </summary>
public sealed class UserGroupPartitionsQueryHandler(
    ActorService actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserGroupPartitionsQuery, IReadOnlyCollection<PartitionInformation>?>
{
    /// <summary>
    /// Returns the partitions that directly contain the user group, filtered by the current
    /// actor's partition accesses. Admins and system actors receive all partitions unfiltered.
    /// Returns <see langword="null"/> if the user group does not exist.
    /// </summary>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    public async Task<IReadOnlyCollection<PartitionInformation>?> Handle(
        UserGroupPartitionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var filter = (actor.IsAdmin || actor.IsSystem) ? null : actor.PartitionAccessesGuids;

        return await userGroupRepository.GetPartitions(query.UserGroupGuid, filter, cancellationToken);
    }
}
