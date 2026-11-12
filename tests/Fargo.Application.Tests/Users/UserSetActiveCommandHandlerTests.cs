using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.Users;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Users;

public class UserSetActiveCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly TestCurrentActor currentActor = new();

    private UserSetActiveCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            userRepository,
            currentActor);

    private void Existing(User user)
        => userRepository.GetByGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

    [Fact]
    public async Task HandleAsync_WhenDeactivating_ShouldDeactivateUser()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateUser]);
        var user = Actor.NewUser(new Nameid("target")).User!;
        Existing(user);

        await CreateHandler().HandleAsync(new UserSetActiveCommand(user.ActorGuid, false));

        Assert.False(user.Actor.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WhenActivating_ShouldActivateUser()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateUser]);
        var user = Actor.NewUser(new Nameid("target")).User!;
        user.Actor.Deactivate();
        Existing(user);

        await CreateHandler().HandleAsync(new UserSetActiveCommand(user.ActorGuid, true));

        Assert.True(user.Actor.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditUser_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUser]);
        var user = Actor.NewUser(new Nameid("target")).User!;
        Existing(user);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetActiveCommand(user.ActorGuid, false)));

        Assert.True(user.Actor.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateUser]);
        var missing = Guid.NewGuid();
        userRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetActiveCommand(missing, false)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ActivateUser], partitionAccesses: [Guid.NewGuid()]);
        var user = Actor.NewUser(new Nameid("target")).User!;
        Existing(user);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetActiveCommand(user.ActorGuid, false)));
    }

    [Fact]
    public async Task HandleAsync_WhenDeactivatingAdministrator_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateUser]);
        var admin = Actor.NewAdministratorUser(new Nameid("admin")).User!;
        Existing(admin);

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserSetActiveCommand(admin.ActorGuid, false)));

        Assert.True(admin.Actor.IsActive);
    }
}
