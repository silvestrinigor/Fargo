using Fargo.Domain.Repositories;
using Fargo.Domain.Security;

namespace Fargo.Domain.Services;

/// <summary>
/// Provides operations for building <see cref="UserActor"/> instances,
/// aggregating user permissions and partition access.
/// </summary>
public class ActorService(
        IUserRepository userRepository,
        IPartitionRepository partitionRepository
        )
{
    /// <summary>
    /// Retrieves a <see cref="UserActor"/> by the user's GUID.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="UserActor"/> containing the user and all accessible partitions,
    /// or <c>null</c> if the user does not exist.
    /// </returns>
    /// <remarks>
    /// This method aggregates partition access from:
    /// <list type="bullet">
    /// <item><description>Direct user partition accesses</description></item>
    /// <item><description>Partition accesses inherited from user groups</description></item>
    /// </list>
    /// It then resolves all descendant partitions for those accesses.
    /// </remarks>
    public async Task<UserActor?> GetUserActorByGuid(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByGuid(userGuid, cancellationToken);

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
