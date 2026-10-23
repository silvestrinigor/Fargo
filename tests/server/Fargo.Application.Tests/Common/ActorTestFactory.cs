using Fargo.Core.Actors;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Users;
using NSubstitute;

namespace Fargo.Application.Tests.Common;

/// <summary>
/// Builds a real <see cref="ActorResolver"/> backed by a mocked
/// <see cref="IUserRepository"/> so handler tests can obtain a resolved
/// <see cref="Actor"/> with precise permissions and partition access.
/// </summary>
/// <remarks>
/// The <see cref="Actor"/> type has an internal constructor, so tests cannot
/// construct one directly. Instead they configure the repository the resolver
/// reads from; the resolver then produces a genuine actor.
/// </remarks>
public static class ActorTestFactory
{
    /// <summary>
    /// Configures <paramref name="userRepository"/> so that resolving the given
    /// actor GUID returns an active user actor with the specified permissions and
    /// partition access. Returns the actor GUID for convenience.
    /// </summary>
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
        var user = User.CreateUser(new Nameid("tester"));

        userRepository
            .GetByGuidAsync(guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        userRepository
            .GetAllActivePermissionsFromUserAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ActionType>>(permissions ?? []));

        userRepository
            .GetAllActivePartitionAccessGuidsFromUserAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>(
                partitionAccesses ?? [FargoCoreWellKnowGuids.GlobalPartitionGuid]));

        return guid;
    }

    /// <summary>
    /// Configures the repository so that resolving the actor GUID returns null
    /// (actor not found / inactive).
    /// </summary>
    public static void ConfigureMissingActor(IUserRepository userRepository, Guid actorGuid)
    {
        userRepository
            .GetByGuidAsync(actorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));
    }

    /// <summary>
    /// Creates a real <see cref="ActorResolver"/> over the given repository.
    /// </summary>
    public static ActorResolver Resolver(IUserRepository userRepository)
        => new(userRepository);
}

/// <summary>
/// A simple <see cref="ICurrentActor"/> stub for tests.
/// </summary>
public sealed class TestCurrentActor : Fargo.Application.Common.ICurrentActor
{
    public Guid Guid { get; init; } = Guid.NewGuid();

    public ActorType ActorType { get; init; } = ActorType.User;

    public bool IsAuthenticated { get; init; } = true;
}
