using Fargo.Application.Common.Actors;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Common;

/// <summary>
/// Builds a real <see cref="ActorResolver"/> backed by a mocked
/// <see cref="IUserRepository"/> so handler tests can obtain a resolved
/// <see cref="SecurityPrincipal"/> with precise permissions and partition access.
/// </summary>
/// <remarks>
/// The <see cref="SecurityPrincipal"/> type has an internal constructor, so tests cannot
/// construct one directly. Instead they configure the repository the resolver
/// reads from; the resolver then produces a genuine actor.
/// </remarks>
public static class ActorTestFactory
{
    public static Guid ConfigureUserActor(
        IUserRepository userRepository,
        Guid? actorGuid = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        IReadOnlyCollection<Guid>? partitionAccesses = null)
    {
        var guid = actorGuid ?? Guid.NewGuid();

        // CreateUser yields an active user; only its existence and active flag matter here.
        var user = Actor.NewUser(new Nameid("tester")).User!;

        userRepository
            .GetByGuidAsync(guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        userRepository
            .GetAllActivePermissionsFromUserAsync(guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ActionType>>(permissions ?? []));

        userRepository
            .GetAllActivePartitionAccessGuidsFromUserAsync(guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>(
                partitionAccesses ?? [FargoCoreWellKnowGuids.GlobalPartitionGuid]));

        return guid;
    }

    public static void ConfigureMissingActor(IUserRepository userRepository, Guid actorGuid)
    {
        userRepository
            .GetByGuidAsync(actorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));
    }

    // User-actor tests do not exercise the service account repository, but the provider needs one.
    public static ActorResolver Resolver(IUserRepository userRepository)
        => Resolver(userRepository, Substitute.For<IServiceAccountRepository>());

    public static ActorResolver Resolver(
        IUserRepository userRepository,
        IServiceAccountRepository serviceAccountRepository)
        => new(new RepositoryAuthorizationProvider(userRepository, serviceAccountRepository));

    /// <summary>
    /// Answers the authorization port from the mocked repositories, without any caching.
    /// </summary>
    private sealed class RepositoryAuthorizationProvider(
        IUserRepository userRepository,
        IServiceAccountRepository serviceAccountRepository) : IActorAuthorizationProvider
    {
        public async Task<ActorAuthorization?> GetAsync(
            Guid actorGuid,
            ActorType actorType,
            CancellationToken cancellationToken = default)
        {
            if (actorType == ActorType.Service)
            {
                var service = await serviceAccountRepository.GetByGuidAsync(actorGuid, cancellationToken);

                if (service is null || !service.Actor.IsActive)
                {
                    return null;
                }

                var accesses = await serviceAccountRepository.GetAllActivePartitionAccessGuidsAsync(actorGuid, cancellationToken);

                return new ActorAuthorization([.. service.Actor.Permissions], [.. accesses]);
            }

            var user = await userRepository.GetByGuidAsync(actorGuid, cancellationToken);

            if (user is null || !user.Actor.IsActive)
            {
                return null;
            }

            var permissions = await userRepository.GetAllActivePermissionsFromUserAsync(actorGuid, cancellationToken);
            var partitionAccesses = await userRepository.GetAllActivePartitionAccessGuidsFromUserAsync(actorGuid, cancellationToken);

            return new ActorAuthorization([.. permissions], [.. partitionAccesses]);
        }
    }
}

public sealed class TestCurrentActor : ICurrentActor
{
    public Guid Guid { get; init; } = Guid.NewGuid();

    public ActorType ActorType { get; init; } = ActorType.User;

    public bool IsAuthenticated { get; init; } = true;
}

public static class UserGroupRepositoryStubs
{
    /// <summary>
    /// Makes the repository report these permissions as the ones <paramref name="userGroup"/> passes to
    /// its members (its own and its ancestors'), as the single recursive query does.
    /// </summary>
    public static void StubInheritedPermissions(
        this Fargo.Core.Entities.UserGroups.IUserGroupRepository repository,
        Fargo.Core.Entities.UserGroups.UserGroup userGroup,
        params ActionType[] permissions)
        => repository.GetInheritedPermissionsAsync(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ActionType>>(permissions));
}
