using Fargo.Application.Commands.UserGroups;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.UserGroups;

public class UserGroupParentCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly TestCurrentActor currentActor = new();

    private UserGroupSetParentCommandHandler CreateSetHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new UserGroupService(userGroupRepository),
            userGroupRepository,
            currentActor);

    private UserGroupRemoveParentCommandHandler CreateRemoveHandler()
        => new(ActorTestFactory.Resolver(userRepository), userGroupRepository, currentActor);

    private static UserGroup NewGroup(string name, params ActionType[] permissions)
    {
        var group = UserGroup.CreateUserGroup(new Nameid(name));

        foreach (var permission in permissions)
        {
            group.AddPermission(permission);
        }

        return group;
    }

    private void Existing(UserGroup group)
    {
        userGroupRepository.GetByGuidAsync(group.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(group));
        userGroupRepository.GetDescendantUserGroupGuidsAsync(group.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));
    }

    [Fact]
    public async Task Set_WhenTheActorHoldsTheParentChainPermissions_ShouldSetTheParent()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.ChangeUserGroupParent, ActionType.CreateItem, ActionType.DeleteItem]);
        var parent = NewGroup("parent", ActionType.CreateItem);
        Existing(parent);
        userGroupRepository.StubInheritedPermissions(parent, ActionType.CreateItem, ActionType.DeleteItem);
        var group = NewGroup("child");
        Existing(group);

        await CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(group.Guid, parent.Guid));

        Assert.Equal(parent.Guid, group.ParentUserGroupGuid);
        await userGroupRepository.Received(1).AcquireHierarchyLockAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Set_WhenTheParentHasAPermissionTheActorLacks_ShouldThrowPermissionDenied()
    {
        // Placing a group under a privileged parent would hand that parent's permissions to its members.
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent]);
        var parent = NewGroup("parent", ActionType.DeleteItem);
        Existing(parent);
        userGroupRepository.StubInheritedPermissions(parent, ActionType.DeleteItem);
        var group = NewGroup("child");
        Existing(group);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(group.Guid, parent.Guid)));

        Assert.Null(group.ParentUserGroupGuid);
    }

    [Fact]
    public async Task Set_WhenAnAncestorOfTheParentHasAPermissionTheActorLacks_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent, ActionType.CreateItem]);
        // The parent itself needs only CreateItem; the permission the actor lacks comes from an ancestor.
        var parent = NewGroup("parent", ActionType.CreateItem);
        Existing(parent);
        userGroupRepository.StubInheritedPermissions(parent, ActionType.CreateItem, ActionType.DeleteItem);
        var group = NewGroup("child");
        Existing(group);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(group.Guid, parent.Guid)));
    }

    [Fact]
    public async Task Set_WhenTheParentIsAlreadyTheCurrentOne_ShouldDoNothing()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent]);
        var parent = NewGroup("parent");
        Existing(parent);
        var group = NewGroup("child");
        group.SetParentUserGroup(parent);
        Existing(group);

        await CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(group.Guid, parent.Guid));

        await userGroupRepository.DidNotReceive().AcquireHierarchyLockAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Set_WhenActorOnlyHasEditUserGroup_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUserGroup]);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task Set_WhenTheAssignmentCreatesACycle_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent]);
        var group = NewGroup("group");
        var descendant = NewGroup("descendant");
        Existing(group);
        Existing(descendant);
        userGroupRepository.GetDescendantUserGroupGuidsAsync(group.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([descendant.Guid]));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(group.Guid, descendant.Guid)));

        Assert.Null(group.ParentUserGroupGuid);
    }

    [Fact]
    public async Task Set_WhenTheGroupNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent]);
        var missing = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(missing, Guid.NewGuid())));
    }

    [Fact]
    public async Task Set_WhenTheParentNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent]);
        var group = NewGroup("child");
        Existing(group);
        var missing = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(group.Guid, missing)));
    }

    [Fact]
    public async Task Set_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        // Groups always belong to the global partition, so an actor without access to it cannot reach them.
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent], partitionAccesses: [Guid.NewGuid()]);
        var group = NewGroup("child");
        Existing(group);
        var parent = NewGroup("parent");
        Existing(parent);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateSetHandler().HandleAsync(new UserGroupSetParentCommand(group.Guid, parent.Guid)));
    }

    [Fact]
    public async Task Remove_WhenTheGroupHasAParent_ShouldClearIt()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent]);
        var parent = NewGroup("parent", ActionType.DeleteItem);
        var group = NewGroup("child");
        group.SetParentUserGroup(parent);
        Existing(group);

        await CreateRemoveHandler().HandleAsync(new UserGroupRemoveParentCommand(group.Guid));

        Assert.Null(group.ParentUserGroupGuid);
    }

    [Fact]
    public async Task Remove_WhenTheGroupHasNoParent_ShouldDoNothing()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent]);
        var group = NewGroup("child");
        Existing(group);

        await CreateRemoveHandler().HandleAsync(new UserGroupRemoveParentCommand(group.Guid));

        Assert.Null(group.ParentUserGroupGuid);
    }

    [Fact]
    public async Task Remove_WhenActorOnlyHasEditUserGroup_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUserGroup]);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateRemoveHandler().HandleAsync(new UserGroupRemoveParentCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task Remove_WhenTheGroupNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeUserGroupParent]);
        var missing = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateRemoveHandler().HandleAsync(new UserGroupRemoveParentCommand(missing)));
    }
}
