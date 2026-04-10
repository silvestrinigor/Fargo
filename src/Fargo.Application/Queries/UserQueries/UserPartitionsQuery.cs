using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserQueries;

/// <summary>
/// Query used to retrieve the partitions that directly contain a specific user.
/// </summary>
/// <param name="UserGuid">The unique identifier of the user.</param>
public sealed record UserPartitionsQuery(Guid UserGuid) : IQuery<IReadOnlyCollection<PartitionInformation>?>;

/// <summary>
/// Handles <see cref="UserPartitionsQuery"/>.
/// </summary>
public sealed class UserPartitionsQueryHandler(
    ActorService actorService,
    IUserRepository userRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserPartitionsQuery, IReadOnlyCollection<PartitionInformation>?>
{
    /// <summary>
    /// Returns the partitions that directly contain the user, filtered by the current
    /// actor's partition accesses. Admins and system actors receive all partitions unfiltered.
    /// Returns <see langword="null"/> if the user does not exist.
    /// </summary>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    public async Task<IReadOnlyCollection<PartitionInformation>?> Handle(
        UserPartitionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var filter = (actor.IsAdmin || actor.IsSystem) ? null : actor.PartitionAccesses;

        return await userRepository.GetPartitions(query.UserGuid, filter, cancellationToken);
    }
}
