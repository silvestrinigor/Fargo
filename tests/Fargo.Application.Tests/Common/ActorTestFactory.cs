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

        // CreateUser yields an active user; the resolver only requires the user
        // to exist and be active. The user's own GUID is irrelevant to the actor
        // GUID that the resolver reports, but GetByGuidAsync must return non-null.
        var user = Actor.NewUser(new Nameid("tester")).User!;

        userRepository
            .GetByGuidAsync(guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        userRepository
            .GetAllActivePermissionsFromUserAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ActionType>>(permissions ?? []));

        userRepository
            .GetAllActivePartitionAccessGuidsFromUserAsync(user.ActorGuid, Arg.Any<CancellationToken>())
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

    /// <summary>
    /// Creates a real <see cref="ActorResolver"/> over the given repository.
    /// A mocked <see cref="IServiceAccountRepository"/> is supplied because the
    /// resolver requires one; user-actor tests do not exercise it.
    /// </summary>
    public static ActorResolver Resolver(IUserRepository userRepository)
        => new(userRepository, Substitute.For<IServiceAccountRepository>());

    public static ActorResolver Resolver(
        IUserRepository userRepository,
        IServiceAccountRepository serviceAccountRepository)
        => new(userRepository, serviceAccountRepository);
}

public sealed class TestCurrentActor : ICurrentActor
{
    public Guid Guid { get; init; } = Guid.NewGuid();

    public ActorType ActorType { get; init; } = ActorType.User;

    public bool IsAuthenticated { get; init; } = true;
}
