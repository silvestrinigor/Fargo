using Fargo.Application.Commands.UserGroups;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.UserGroups;

public class UserGroupSetPermissionsCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly TestCurrentActor currentActor = new();

    private UserGroupSetPermissionsCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            userGroupRepository,
            currentActor);

    private static UserGroup NewUserGroup()
        => UserGroup.CreateUserGroup(new Nameid("editors"));

    private void Existing(UserGroup userGroup)
        => userGroupRepository.GetByGuidAsync(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(userGroup));

    [Fact]
    public async Task HandleAsync_WhenGrantingAndRevoking_ShouldApplyBoth()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.ChangeUserGroupPermissions, ActionType.CreateItem, ActionType.DeleteItem]);
        var userGroup = NewUserGroup();
        userGroup.AddPermission(ActionType.DeleteItem);
        Existing(userGroup);

        await CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(
            userGroup.Guid,
            new PermissionsChangeDto(
                PermissionsToAdd: [ActionType.CreateItem],
                PermissionsToRemove: [ActionType.DeleteItem])));

        Assert.Contains(ActionType.CreateItem, userGroup.Permissions);
        Assert.DoesNotContain(ActionType.DeleteItem, userGroup.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditUserGroup_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditUserGroup, ActionType.CreateItem]);
        var userGroup = NewUserGroup();
        Existing(userGroup);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(
                userGroup.Guid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));

        Assert.Empty(userGroup.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenGrantingAPermissionTheActorDoesNotHold_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupPermissions]);
        var userGroup = NewUserGroup();
        Existing(userGroup);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(
                userGroup.Guid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.DeleteItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenRevokingAPermissionTheActorDoesNotHold_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupPermissions]);
        var userGroup = NewUserGroup();
        userGroup.AddPermission(ActionType.DeleteItem);
        Existing(userGroup);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(
                userGroup.Guid, new PermissionsChangeDto(PermissionsToRemove: [ActionType.DeleteItem]))));

        Assert.Contains(ActionType.DeleteItem, userGroup.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenSamePermissionIsAddedAndRemoved_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupPermissions, ActionType.CreateItem]);
        var userGroup = NewUserGroup();
        Existing(userGroup);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(
                userGroup.Guid,
                new PermissionsChangeDto(
                    PermissionsToAdd: [ActionType.CreateItem],
                    PermissionsToRemove: [ActionType.CreateItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsRequested_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupPermissions]);
        var userGroup = NewUserGroup();
        Existing(userGroup);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(userGroup.Guid, new PermissionsChangeDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenUserGroupNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupPermissions, ActionType.CreateItem]);
        var missing = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(
                missing, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.ChangeUserGroupPermissions, ActionType.CreateItem],
            partitionAccesses: [Guid.NewGuid()]);
        var userGroup = NewUserGroup();
        Existing(userGroup);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(
                userGroup.Guid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenRevokingFromAdministratorsGroup_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupPermissions, ActionType.CreateItem]);
        var administrators = UserGroup.CreateAdministratorsUserGroup(new Nameid("administrators"));
        administrators.AddPermission(ActionType.CreateItem);
        Existing(administrators);

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPermissionsCommand(
                administrators.Guid, new PermissionsChangeDto(PermissionsToRemove: [ActionType.CreateItem]))));

        Assert.Contains(ActionType.CreateItem, administrators.Permissions);
    }
}
