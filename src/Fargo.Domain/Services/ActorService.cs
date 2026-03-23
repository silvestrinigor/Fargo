using Fargo.Domain.Repositories;
using Fargo.Domain.Security;

namespace Fargo.Domain.Services;

public class ActorService(
        IUserRepository userRepository,
        IPartitionRepository partitionRepository
        )
{
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
