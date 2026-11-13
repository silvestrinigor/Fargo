using Fargo.Application.Commands.Users;
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

namespace Fargo.Application.Tests.Users;

public class UserSetUserGroupsCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly TestCurrentActor currentActor = new();

    private UserSetUserGroupsCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            userRepository,
            userGroupRepository,
            currentActor);

    private static User NewTarget()
        => Actor.NewUser(new Nameid("target")).User!;

    private static UserGroup NewGroup(string name, params ActionType[] permissions)
    {
        var group = UserGroup.CreateUserGroup(new Nameid(name));

        foreach (var permission in permissions)
        {
            group.AddPermission(permission);
        }

        return group;
    }

    private void Existing(User user)
        => userRepository.GetByGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

    private void Existing(UserGroup group)
        => userGroupRepository.GetByGuidAsync(group.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(group));

    private static UserSetUserGroupsCommand Add(User user, UserGroup group)
        => new(user.ActorGuid, new UserGroupsChangeDto(UserGroupsToAdd: [group.Guid]));

    [Fact]
    public async Task HandleAsync_WhenAddingAGroupWhosePermissionsTheActorHolds_ShouldAddTheUser()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers, ActionType.CreateItem]);
        var target = NewTarget();
        Existing(target);
        var group = NewGroup("editors", ActionType.CreateItem);
        Existing(group);
        userGroupRepository.StubInheritedPermissions(group, ActionType.CreateItem);

        await CreateHandler().HandleAsync(Add(target, group));

        Assert.Contains(target.UserGroupMemberships, m => m.UserGroupGuid == group.Guid);
    }

    [Fact]
    public async Task HandleAsync_WhenAddingAGroupWhosePermissionsTheActorLacks_ShouldThrowPermissionDenied()
    {
        // The escalation path: an actor that can only change memberships must not hand out
        // permissions it does not hold itself.
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers]);
        var target = NewTarget();
        Existing(target);
        var group = NewGroup("deleters", ActionType.DeleteItem);
        Existing(group);
        userGroupRepository.StubInheritedPermissions(group, ActionType.DeleteItem);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(Add(target, group)));

        Assert.Empty(target.UserGroupMemberships);
    }

    [Fact]
    public async Task HandleAsync_WhenAnAncestorGroupHasAPermissionTheActorLacks_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers, ActionType.CreateItem]);
        var target = NewTarget();
        Existing(target);
        // The group itself needs only CreateItem; the permission the actor lacks comes from an ancestor.
        var group = NewGroup("child", ActionType.CreateItem);
        Existing(group);
        userGroupRepository.StubInheritedPermissions(group, ActionType.CreateItem, ActionType.DeleteItem);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(Add(target, group)));

        Assert.Empty(target.UserGroupMemberships);
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingAGroup_ShouldNotRequireItsPermissions()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers]);
        var target = NewTarget();
        var group = NewGroup("deleters", ActionType.DeleteItem);
        target.AddUserGroup(group);
        Existing(target);
        Existing(group);

        await CreateHandler().HandleAsync(
            new UserSetUserGroupsCommand(target.ActorGuid, new UserGroupsChangeDto(UserGroupsToRemove: [group.Guid])));

        Assert.Empty(target.UserGroupMemberships);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditUser_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUser]);
        var target = NewTarget();
        Existing(target);
        var group = NewGroup("editors");
        Existing(group);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(Add(target, group)));
    }

    [Fact]
    public async Task HandleAsync_WhenSameGroupIsAddedAndRemoved_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers]);
        var target = NewTarget();
        Existing(target);
        var groupGuid = Guid.NewGuid();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetUserGroupsCommand(
                target.ActorGuid, new UserGroupsChangeDto(UserGroupsToAdd: [groupGuid], UserGroupsToRemove: [groupGuid]))));
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsRequested_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers]);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetUserGroupsCommand(Guid.NewGuid(), new UserGroupsChangeDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers]);
        var missing = Guid.NewGuid();
        userRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetUserGroupsCommand(
                missing, new UserGroupsChangeDto(UserGroupsToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenGroupNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers]);
        var target = NewTarget();
        Existing(target);
        var missing = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetUserGroupsCommand(
                target.ActorGuid, new UserGroupsChangeDto(UserGroupsToAdd: [missing]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        // Users and groups always belong to the global partition, so an actor without access to it reaches neither.
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers], partitionAccesses: [Guid.NewGuid()]);
        var target = NewTarget();
        Existing(target);
        var group = NewGroup("editors");
        Existing(group);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(Add(target, group)));
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingTheAdministratorFromTheAdministratorsGroup_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupMembers]);
        var admin = Actor.NewAdministratorUser(new Nameid("admin")).User!;
        Existing(admin);
        var administrators = UserGroup.CreateAdministratorsUserGroup(new Nameid("administrators"));
        Existing(administrators);

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserSetUserGroupsCommand(
                admin.ActorGuid, new UserGroupsChangeDto(UserGroupsToRemove: [administrators.Guid]))));
    }
}
