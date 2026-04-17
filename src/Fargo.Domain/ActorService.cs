using Fargo.Domain.Partitions;
using Fargo.Domain.System;
using Fargo.Domain.Users;

namespace Fargo.Domain;

/// <summary>
/// Service responsible for resolving and constructing <see cref="Actor"/> instances,
/// including <see cref="UserActor"/> and <see cref="SystemActor"/>.
/// </summary>
/// <remarks>
/// This service centralizes the logic for loading users and aggregating their
/// effective partition access based on direct assignments and group memberships.
/// </remarks>
public class ActorService(
        IUserRepository userRepository,
        IPartitionRepository partitionRepository
        )
{
    /// <summary>
    /// Retrieves an <see cref="Actor"/> instance based on its unique identifier.
    /// </summary>
    /// <param name="actorGuid">The unique identifier of the actor.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="System.SystemActor"/> if the identifier matches the system actor;
    /// a <see cref="UserActor"/> if a corresponding user is found;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// For user actors, this method resolves effective partition access by:
    /// <list type="bullet">
    /// <item><description>Including partitions directly assigned to the user</description></item>
    /// <item><description>Including partitions inherited through user group memberships</description></item>
    /// <item><description>Expanding all collected partitions to include their descendants</description></item>
    /// </list>
    /// This ensures the returned actor contains the complete set of accessible partitions.
    /// </remarks>
    public async Task<Actor?> GetActorByGuid(
        Guid actorGuid,
        CancellationToken cancellationToken = default)
    {
        if (actorGuid == SystemService.SystemGuid)
        {
            var systemActor = new SystemActor();

            return systemActor;
        }

        var user = await userRepository.GetByGuid(actorGuid, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var parentPartitions = user.PartitionAccesses
            .Select(p => p.PartitionGuid)
            .ToHashSet();

        parentPartitions.UnionWith(
            user.UserGroups
                .SelectMany(g => g.PartitionAccesses)
                .Select(p => p.PartitionGuid)
        );

        var partitionAccess = await partitionRepository
            .GetDescendantGuids(parentPartitions, true, cancellationToken);

        return new UserActor(user, partitionAccess);
    }
}
