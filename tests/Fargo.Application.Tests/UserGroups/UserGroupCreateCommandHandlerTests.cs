using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.UserGroups;
using Fargo.Application.Dtos.UserGroups;
using Fargo.Application.Persistence;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.UserGroups;

public class UserGroupCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private UserGroupCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new UserGroupService(userGroupRepository),
            userGroupRepository,
            partitionRepository,
            currentActor);

    private static Partition GlobalPartition()
        => Partition.CreateGlobalPartition(new Name("Global"));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddUserGroupAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var command = new UserGroupCreateCommand(new UserGroupCreateDto(new Nameid("editors")));

        await CreateHandler().HandleAsync(command);

        userGroupRepository.Received(1).Add(Arg.Any<UserGroup>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(new UserGroupCreateDto(new Nameid("editors")))));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(new UserGroupCreateDto(new Nameid("editors")))));
    }

    [Fact]
    public async Task HandleAsync_WhenNameidAlreadyExists_ShouldThrowAndNotSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(new UserGroupCreateDto(new Nameid("editors")))));

        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenParentUserGroupNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup, ActionType.ChangeUserGroupParent]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var missingParent = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missingParent, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(
                    new UserGroupCreateDto(new Nameid("editors"), ParentUserGroup: missingParent))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToRequestedPartition_ShouldThrowAccessDenied()
    {
        // Actor has the permission and global access, but the requested partition
        // is a different one the actor cannot access.
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup, ActionType.ChangeUserGroupPartitions]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var otherPartition = Partition.CreatePartition(new Name("Other"), GlobalPartition());
        partitionRepository.GetByGuidAsync(otherPartition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(otherPartition));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(
                    new UserGroupCreateDto(new Nameid("editors"), PartitionsToAdd: [otherPartition.Guid]))));

        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGrantingPermissionsWithoutChangeUserGroupPermissions_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.CreateUserGroup, ActionType.CreateItem]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var command = new UserGroupCreateCommand(
            new UserGroupCreateDto(new Nameid("editors"), PermissionsToAdd: [ActionType.CreateItem]));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));

        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
    }

    [Fact]
    public async Task HandleAsync_WhenGrantingPermissionsWithChangeUserGroupPermissions_ShouldGrantThem()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.CreateUserGroup, ActionType.ChangeUserGroupPermissions, ActionType.CreateItem]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var command = new UserGroupCreateCommand(
            new UserGroupCreateDto(new Nameid("editors"), PermissionsToAdd: [ActionType.CreateItem]));

        await CreateHandler().HandleAsync(command);

        userGroupRepository.Received(1).Add(Arg.Is<UserGroup>(g => g!.Permissions.Contains(ActionType.CreateItem)));
    }

    [Fact]
    public async Task HandleAsync_WhenSettingAParentWithoutChangeUserGroupParent_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var parent = UserGroup.CreateUserGroup(new Nameid("parent"));
        userGroupRepository.GetByGuidAsync(parent.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(parent));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupCreateCommand(
                new UserGroupCreateDto(new Nameid("editors"), ParentUserGroup: parent.Guid))));

        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheParentHasAPermissionTheActorLacks_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.CreateUserGroup, ActionType.ChangeUserGroupParent]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var parent = UserGroup.CreateUserGroup(new Nameid("parent"));
        parent.AddPermission(ActionType.DeleteItem);
        userGroupRepository.GetByGuidAsync(parent.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(parent));
        userGroupRepository.StubInheritedPermissions(parent, ActionType.DeleteItem);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupCreateCommand(
                new UserGroupCreateDto(new Nameid("editors"), ParentUserGroup: parent.Guid))));

        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
    }

    [Fact]
    public async Task HandleAsync_WhenSettingAParentTheActorMayGrant_ShouldSetIt()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.CreateUserGroup, ActionType.ChangeUserGroupParent, ActionType.CreateItem]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var parent = UserGroup.CreateUserGroup(new Nameid("parent"));
        parent.AddPermission(ActionType.CreateItem);
        userGroupRepository.GetByGuidAsync(parent.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(parent));
        userGroupRepository.StubInheritedPermissions(parent, ActionType.CreateItem);

        await CreateHandler().HandleAsync(new UserGroupCreateCommand(
            new UserGroupCreateDto(new Nameid("editors"), ParentUserGroup: parent.Guid)));

        userGroupRepository.Received(1).Add(Arg.Is<UserGroup>(g => g!.ParentUserGroupGuid == parent.Guid));
    }

    [Fact]
    public async Task HandleAsync_WhenAddingPartitionsWithoutChangeUserGroupPartitions_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupCreateCommand(
                new UserGroupCreateDto(new Nameid("editors"), PartitionAccessesToAdd: [Guid.NewGuid()]))));

        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
    }
}
