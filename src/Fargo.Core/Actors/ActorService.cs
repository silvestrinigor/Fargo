using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Users;

namespace Fargo.Core.Actors;

public sealed class ActorService(
    IUserRepository userRepository, IPartitionRepository partitionRepository) : IActorService
{
    public async Task<Actor?> GetActorByActorIdAsync(ActorId actorId, CancellationToken cancellationToken = default)
    {
        if (actorId.ActorType == ActorType.User)
        {
            return await GetUserActorByGuid(actorId.Guid, cancellationToken);
        }

        else if (actorId.ActorType == ActorType.Application)
        {
            return await GetApplicationActorByGuid(actorId.Guid, cancellationToken);
        }

        return null;
    }

    private async Task<Actor?> GetApplicationActorByGuid(Guid guid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<Actor?> GetUserActorByGuid(Guid guid, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByGuidAsync(guid, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var partitionAccessGuids = user.PartitionAccesses
            .Select(p => p.PartitionGuid)
            .ToHashSet();

        partitionAccessGuids.UnionWith(
            user.UserGroups
                .Where(g => g.IsActive)
                .SelectMany(g => g.PartitionAccesses)
                .Select(p => p.PartitionGuid));

        var partitionDescendantAccessGuids = await partitionRepository.GetDescendantGuids(
            partitionGuids: partitionAccessGuids,
            includeRoots: true,
            cancellationToken);

        var permissions = new HashSet<ActionType>(
            user.Permissions.Select(
                permission => permission.Action));

        foreach (var group in user.UserGroups.Where(group => group.IsActive))
        {
            permissions.UnionWith(
                group.Permissions.Select(permission => permission.Action));
        }

        var actorId = new ActorId(user.Guid, ActorType.User);

        return new Actor(actorId, permissions, partitionDescendantAccessGuids.ToHashSet());
    }
}
