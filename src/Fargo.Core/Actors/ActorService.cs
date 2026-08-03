using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Users;

namespace Fargo.Core.Actors;

public sealed class ActorService(IUserRepository userRepository, IPartitionRepository partitionRepository)
{
    public async Task<Actor?> GetActorByGuidAndTypeAsync(Guid actorGuid, ActorType actorType, CancellationToken cancellationToken = default)
    {
        if (actorType == ActorType.User)
        {
            return await GetUserActorByGuidAsync(actorGuid, cancellationToken);
        }

        return null;
    }

    private async Task<Actor?> GetUserActorByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByGuidAsync(guid, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var partitionAccessGuids = user.PartitionAccesses
            .Select(p => p.Guid)
            .ToHashSet();

        partitionAccessGuids.UnionWith(
            user.UserGroups
                .Where(g => g.IsActive)
                .SelectMany(g => g.PartitionAccesses)
                .Select(p => p.Guid));

        var partitionDescendantAccessGuids = await partitionRepository.GetDescendantGuidsAsync(
            partitionGuids: partitionAccessGuids,
            includeRoots: true,
            cancellationToken);

        var permissions = new HashSet<ActionType>(user.Permissions);

        foreach (var group in user.UserGroups.Where(group => group.IsActive))
        {
            permissions.UnionWith(group.Permissions);
        }

        return new Actor(user.Guid, ActorType.User, permissions, partitionDescendantAccessGuids.ToHashSet());
    }
}
