using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.Users;
using Fargo.Application.Dtos.Users;
using Fargo.Application.Persistence;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Users;

public class UserCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private UserCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new UserService(userRepository),
            userRepository,
            partitionRepository,
            userGroupRepository,
            currentActor,
            passwordHasher,
            TimeProvider.System);

    private static UserCreateCommand SimpleCommand(string nameid = "newuser")
        => new(new UserCreateDto(new Nameid(nameid)));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddUserAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        await CreateHandler().HandleAsync(SimpleCommand());

        userRepository.Received(1).Add(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(SimpleCommand()));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(SimpleCommand()));
    }

    [Fact]
    public async Task HandleAsync_WhenNameidAlreadyExists_ShouldThrowDomainValidation()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(SimpleCommand()));

        userRepository.DidNotReceive().Add(Arg.Any<User>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRequestedPartitionNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser, ActionType.ChangeUserPartitions]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var missingPartition = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missingPartition, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), PartitionsToAdd: [missingPartition]));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToRequestedPartition_ShouldThrowAccessDenied()
    {
        // Actor has CreateUser but only global partition access; the requested
        // partition is a different one the actor cannot access.
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser, ActionType.ChangeUserPartitions]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var partition = Partition.CreatePartition(new Name("restricted"), global);
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), PartitionsToAdd: [partition.Guid]));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenGrantingPermissionsWithoutChangeUserPermissions_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.CreateUser, ActionType.CreateItem]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), PermissionsToAdd: [ActionType.CreateItem]));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));

        userRepository.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_WhenGrantingPermissionsWithChangeUserPermissions_ShouldGrantThem()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.CreateUser, ActionType.ChangeUserPermissions, ActionType.CreateItem]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), PermissionsToAdd: [ActionType.CreateItem]));

        await CreateHandler().HandleAsync(command);

        userRepository.Received(1).Add(Arg.Is<User>(u => u.Actor.Permissions.Contains(ActionType.CreateItem)));
    }

    [Fact]
    public async Task HandleAsync_WhenAddingPartitionsWithoutChangeUserPartitions_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), PartitionAccessesToAdd: [Guid.NewGuid()]));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));

        userRepository.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_WhenAddingUserGroupsWithoutChangeUserGroupMembers_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), UserGroupsToAdd: [Guid.NewGuid()]));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));

        userRepository.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheUserGroupHasAPermissionTheActorLacks_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.CreateUser, ActionType.ChangeUserGroupMembers]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var group = UserGroup.CreateUserGroup(new Nameid("deleters"));
        group.AddPermission(ActionType.DeleteItem);
        userGroupRepository.GetByGuidAsync(group.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(group));
        userGroupRepository.StubInheritedPermissions(group, ActionType.DeleteItem);

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), UserGroupsToAdd: [group.Guid]));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));

        userRepository.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task HandleAsync_WhenAddingUserGroupsTheActorMayGrant_ShouldAddTheMembership()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.CreateUser, ActionType.ChangeUserGroupMembers, ActionType.CreateItem]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var group = UserGroup.CreateUserGroup(new Nameid("editors"));
        group.AddPermission(ActionType.CreateItem);
        userGroupRepository.GetByGuidAsync(group.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(group));
        userGroupRepository.StubInheritedPermissions(group, ActionType.CreateItem);

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), UserGroupsToAdd: [group.Guid]));

        await CreateHandler().HandleAsync(command);

        userRepository.Received(1).Add(
            Arg.Is<User>(u => u.UserGroupMemberships.Any(m => m.UserGroupGuid == group.Guid)));
    }
}
