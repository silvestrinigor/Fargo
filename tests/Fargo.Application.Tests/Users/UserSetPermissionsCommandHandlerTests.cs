using Fargo.Application.Commands.Users;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Users;

public class UserSetPermissionsCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly TestCurrentActor currentActor = new();

    private UserSetPermissionsCommandHandler CreateHandler(ICurrentActor? actor = null)
        => new(
            ActorTestFactory.Resolver(userRepository),
            userRepository,
            actor ?? currentActor);

    private void Existing(User user)
        => userRepository.GetByGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

    private static User NewTarget()
        => Actor.NewUser(new Nameid("target")).User!;

    [Fact]
    public async Task HandleAsync_WhenGrantingAndRevoking_ShouldApplyBoth()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.ChangeUserPermissions, ActionType.CreateItem, ActionType.DeleteItem]);
        var target = NewTarget();
        target.Actor.AddPermission(ActionType.DeleteItem);
        Existing(target);

        await CreateHandler().HandleAsync(new UserSetPermissionsCommand(
            target.ActorGuid,
            new PermissionsChangeDto(
                PermissionsToAdd: [ActionType.CreateItem],
                PermissionsToRemove: [ActionType.DeleteItem])));

        Assert.Contains(ActionType.CreateItem, target.Actor.Permissions);
        Assert.DoesNotContain(ActionType.DeleteItem, target.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditUser_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditUser, ActionType.CreateItem]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPermissionsCommand(
                target.ActorGuid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));

        Assert.Empty(target.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenGrantingAPermissionTheActorDoesNotHold_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserPermissions]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPermissionsCommand(
                target.ActorGuid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.DeleteItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenRevokingAPermissionTheActorDoesNotHold_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserPermissions]);
        var target = NewTarget();
        target.Actor.AddPermission(ActionType.DeleteItem);
        Existing(target);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPermissionsCommand(
                target.ActorGuid, new PermissionsChangeDto(PermissionsToRemove: [ActionType.DeleteItem]))));

        Assert.Contains(ActionType.DeleteItem, target.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenSamePermissionIsAddedAndRemoved_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserPermissions, ActionType.CreateItem]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPermissionsCommand(
                target.ActorGuid,
                new PermissionsChangeDto(
                    PermissionsToAdd: [ActionType.CreateItem],
                    PermissionsToRemove: [ActionType.CreateItem]))));

        Assert.Empty(target.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsRequested_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserPermissions]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPermissionsCommand(target.ActorGuid, new PermissionsChangeDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenActorChangesItsOwnPermissions_ShouldThrow()
    {
        var self = NewTarget();
        var selfActor = new TestCurrentActor { Guid = self.ActorGuid };
        ActorTestFactory.ConfigureUserActor(
            userRepository, self.ActorGuid, [ActionType.ChangeUserPermissions, ActionType.CreateItem]);
        Existing(self);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler(selfActor).HandleAsync(new UserSetPermissionsCommand(
                self.ActorGuid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));

        Assert.Empty(self.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserPermissions, ActionType.CreateItem]);
        var missing = Guid.NewGuid();
        userRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPermissionsCommand(
                missing, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.ChangeUserPermissions, ActionType.CreateItem],
            partitionAccesses: [Guid.NewGuid()]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPermissionsCommand(
                target.ActorGuid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenRevokingFromAdministrator_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserPermissions, ActionType.CreateItem]);
        var admin = Actor.NewAdministratorUser(new Nameid("admin")).User!;
        admin.Actor.AddPermission(ActionType.CreateItem);
        Existing(admin);

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserSetPermissionsCommand(
                admin.ActorGuid, new PermissionsChangeDto(PermissionsToRemove: [ActionType.CreateItem]))));

        Assert.Contains(ActionType.CreateItem, admin.Actor.Permissions);
    }
}
