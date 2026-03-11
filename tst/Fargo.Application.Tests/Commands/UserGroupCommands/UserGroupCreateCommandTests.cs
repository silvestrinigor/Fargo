using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.UserGroupCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.UserGroupCommands;

public sealed class UserGroupCreateCommandHandlerTests
{
    private readonly UserGroupService userGroupService;
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly UserGroupCreateCommandHandler handler;

    public UserGroupCreateCommandHandlerTests()
    {
        userGroupService = new UserGroupService(userGroupRepository);

        handler = new UserGroupCreateCommandHandler(
            userGroupService,
            userGroupRepository,
            userRepository,
            unitOfWork,
            currentUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var action = async () => await handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(action);

        await userRepository.Received(1).GetByGuid(actorGuid, Arg.Any<CancellationToken>());
        await userGroupRepository.DidNotReceive().ExistsByNameid(Arg.Any<Nameid>(), Arg.Any<CancellationToken>());
        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserInactiveFargoDomainException_When_ActorIsInactive()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(isActive: false);
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        // Act
        var action = async () => await handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserInactiveFargoDomainException>(action);

        await userRepository.Received(1).GetByGuid(actorGuid, Arg.Any<CancellationToken>());
        await userGroupRepository.DidNotReceive().ExistsByNameid(Arg.Any<Nameid>(), Arg.Any<CancellationToken>());
        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(isActive: true);
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        // Act
        var action = async () => await handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(action);

        await userRepository.Received(1).GetByGuid(actorGuid, Arg.Any<CancellationToken>());
        await userGroupRepository.DidNotReceive().ExistsByNameid(Arg.Any<Nameid>(), Arg.Any<CancellationToken>());
        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CreateUserGroupWithDescriptionAndPermissions_When_CommandIsValid()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(
            isActive: true,
            permissions:
            [
                ActionType.CreateUserGroup
            ]);

        var command = CreateCommand(
            nameid: new Nameid("admins"),
            description: new Description("Administrative group"),
            permissions:
            [
                new UserGroupPermissionUpdateModel(ActionType.CreateUser),
                new UserGroupPermissionUpdateModel(ActionType.EditUser)
            ]);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>())
            .Returns(false);

        UserGroup? addedUserGroup = null;

        userGroupRepository
            .When(x => x.Add(Arg.Any<UserGroup>()))
            .Do(callInfo => addedUserGroup = callInfo.Arg<UserGroup>());

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.NotEqual(Guid.Empty, result);

        await userRepository.Received(1).GetByGuid(actorGuid, Arg.Any<CancellationToken>());
        await userGroupRepository.Received(1).ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>());
        userGroupRepository.Received(1).Add(Arg.Any<UserGroup>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedUserGroup);
        Assert.Equal(result, addedUserGroup!.Guid);
        Assert.Equal(command.UserGroup.Nameid, addedUserGroup.Nameid);
        Assert.Equal(command.UserGroup.Description, addedUserGroup.Description);
        Assert.True(addedUserGroup.IsActive);
        Assert.Equal(2, addedUserGroup.UserGroupPermissions.Count);

        Assert.Contains(
            addedUserGroup.UserGroupPermissions,
            x => x.Action == ActionType.CreateUser);

        Assert.Contains(
            addedUserGroup.UserGroupPermissions,
            x => x.Action == ActionType.EditUser);
    }

    [Fact]
    public async Task Handle_Should_CreateUserGroupWithEmptyDescription_When_DescriptionIsNull()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(
            isActive: true,
            permissions:
            [
                ActionType.CreateUserGroup
            ]);

        var command = CreateCommand(
            description: null,
            permissions: null);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>())
            .Returns(false);

        UserGroup? addedUserGroup = null;

        userGroupRepository
            .When(x => x.Add(Arg.Any<UserGroup>()))
            .Do(callInfo => addedUserGroup = callInfo.Arg<UserGroup>());

        // Act
        var result = await handler.Handle(command);

        // Assert
        await userRepository.Received(1).GetByGuid(actorGuid, Arg.Any<CancellationToken>());
        await userGroupRepository.Received(1).ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>());
        userGroupRepository.Received(1).Add(Arg.Any<UserGroup>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedUserGroup);
        Assert.Equal(result, addedUserGroup!.Guid);
        Assert.Equal(command.UserGroup.Nameid, addedUserGroup.Nameid);
        Assert.Equal(Description.Empty, addedUserGroup.Description);
        Assert.True(addedUserGroup.IsActive);
        Assert.Empty(addedUserGroup.UserGroupPermissions);
    }

    [Fact]
    public async Task Handle_Should_CreateUserGroupWithoutPermissions_When_PermissionsIsEmpty()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(
            isActive: true,
            permissions:
            [
                ActionType.CreateUserGroup
            ]);

        var command = CreateCommand(
            permissions: []);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>())
            .Returns(false);

        UserGroup? addedUserGroup = null;

        userGroupRepository
            .When(x => x.Add(Arg.Any<UserGroup>()))
            .Do(callInfo => addedUserGroup = callInfo.Arg<UserGroup>());

        // Act
        var result = await handler.Handle(command);

        // Assert
        await userRepository.Received(1).GetByGuid(actorGuid, Arg.Any<CancellationToken>());
        await userGroupRepository.Received(1).ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>());
        userGroupRepository.Received(1).Add(Arg.Any<UserGroup>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedUserGroup);
        Assert.Equal(result, addedUserGroup!.Guid);
        Assert.Empty(addedUserGroup.UserGroupPermissions);
    }

    [Fact]
    public async Task Handle_Should_NotAddDuplicatePermissions_When_CommandContainsRepeatedPermissions()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(
            isActive: true,
            permissions:
            [
                ActionType.CreateUserGroup
            ]);

        var command = CreateCommand(
            permissions:
            [
                new UserGroupPermissionUpdateModel(ActionType.CreateUser),
                new UserGroupPermissionUpdateModel(ActionType.CreateUser)
            ]);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>())
            .Returns(false);

        UserGroup? addedUserGroup = null;

        userGroupRepository
            .When(x => x.Add(Arg.Any<UserGroup>()))
            .Do(callInfo => addedUserGroup = callInfo.Arg<UserGroup>());

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedUserGroup);
        Assert.Single(addedUserGroup!.UserGroupPermissions);
        Assert.Contains(
            addedUserGroup.UserGroupPermissions,
            x => x.Action == ActionType.CreateUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowUserGroupNameidAlreadyExistsDomainException_When_NameidAlreadyExists()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(
            isActive: true,
            permissions:
            [
                ActionType.CreateUserGroup
            ]);

        var command = CreateCommand();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var action = async () => await handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserGroupNameidAlreadyExistsDomainException>(action);

        await userRepository.Received(1).GetByGuid(actorGuid, Arg.Any<CancellationToken>());
        await userGroupRepository.Received(1).ExistsByNameid(command.UserGroup.Nameid, Arg.Any<CancellationToken>());
        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    private static UserGroupCreateCommand CreateCommand(
        Nameid? nameid = null,
        Description? description = null,
        IEnumerable<UserGroupPermissionUpdateModel>? permissions = null)
    {
        return new UserGroupCreateCommand(
            new UserGroupCreateModel(
                nameid ?? new Nameid("group-admin"),
                description,
                permissions?.ToArray()));
    }

    private static User CreateUser(
        bool isActive = true,
        IEnumerable<ActionType>? permissions = null)
    {
        var user = new User
        {
            Nameid = new Nameid("actor"),
            PasswordHash = new PasswordHash("hashedasdfasdfasdfasdfasdfasdfksajflakfjalfdjasldfjdsakfdasklfdjasfklasdkfj")
        };

        if (!isActive)
        {
            user.IsActive = false;
        }

        foreach (var permission in permissions ?? [])
        {
            user.AddPermission(permission);
        }

        return user;
    }
}