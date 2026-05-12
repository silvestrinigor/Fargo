using Fargo.Application.Identity;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.Users;

namespace Fargo.Infrastructure.Security;

public sealed class AuthorizationContextFactory(
    IUserRepository userRepository,
    IPartitionRepository partitionRepository
) : IAuthorizationContextFactory
{
    public async Task<IAuthorizationContext> CreateFromUserGuid(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByGuid(userGuid, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessFargoApplicationException();
        }

        return await CreateFromUser(user, cancellationToken);
    }

    public async Task<IAuthorizationContext> CreateFromUser(
        User user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var partitionGuids = user.PartitionAccesses
            .Select(p => p.PartitionGuid)
            .ToHashSet();

        partitionGuids.UnionWith(
            user.UserGroups
                .Where(group => group.IsActive)
                .SelectMany(group => group.PartitionAccesses)
                .Select(p => p.PartitionGuid));

        var partitionAccesses = await partitionRepository.GetDescendantGuids(
            partitionGuids: partitionGuids,
            includeRoots: true,
            cancellationToken);

        var permissions = new HashSet<ActionType>(
            user.Permissions.Select(permission => permission.Action));

        foreach (var group in user.UserGroups.Where(group => group.IsActive))
        {
            permissions.UnionWith(group.Permissions.Select(permission => permission.Action));
        }

        return new AuthorizationContext(
            user.Guid,
            true,
            user.Guid == UserService.DefaultAdministratorUserGuid,
            permissions.ToArray(),
            partitionAccesses,
            user.UserGroups.Select(group => group.Guid).ToArray());
    }
}
