using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.UserGroupCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.UserGroupCommands;

public sealed class UserGroupUpdateCommandHandlerTests
{
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly UserGroupUpdateCommandHandler handler;

    public UserGroupUpdateCommandHandlerTests()
    {
        handler = new UserGroupUpdateCommandHandler(
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
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(action);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserGroupNotFoundFargoApplicationException_When_UserGroupIsNotFound()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.EditUserGroup);
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(command.UserGroupGuid, Arg.Any<CancellationToken>())
            .Returns((UserGroup?)null);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<UserGroupNotFoundFargoApplicationException>(action);
        Assert.Equal(command.UserGroupGuid, exception.UserGroupGuid);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserInactiveFargoDomainException_When_ActorIsInactive()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.EditUserGroup);
        actor.IsActive = false;

        var userGroup = CreateUserGroup();
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(command.UserGroupGuid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UserInactiveFargoDomainException>(action);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHaveEditUserGroupPermission()
    {
        // Arrange
        var actor = CreateActor();
        var userGroup = CreateUserGroup();
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(command.UserGroupGuid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(action);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateBasicProperties_When_ValuesAreProvided()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.EditUserGroup);
        var userGroup = CreateUserGroup();

        var newNameid = Nameid.FromString("group.updated");
        var newDescription = Description.FromString("Updated description");

        var command = new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateModel(
                Nameid: newNameid,
                Description: newDescription,
                IsActive: null,
                Permissions: null));

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newNameid, userGroup.Nameid);
        Assert.Equal(newDescription, userGroup.Description);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateIsActive_When_ValueIsProvided()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.EditUserGroup);
        var userGroup = CreateUserGroup();
        userGroup.IsActive = true;

        var command = new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateModel(
                IsActive: false));

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(userGroup.IsActive);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_SynchronizePermissions_When_PermissionsAreProvided()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.EditUserGroup);
        var userGroup = CreateUserGroup();

        userGroup.AddPermission(ActionType.CreateUser);
        userGroup.AddPermission(ActionType.DeleteUser);

        var command = new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateModel(
                Permissions:
                [
                    new UserGroupPermissionUpdateModel(ActionType.CreateUser),
                    new UserGroupPermissionUpdateModel(ActionType.EditUser),
                    new UserGroupPermissionUpdateModel(ActionType.EditUser)
                ]));

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var actions = userGroup.Permissions
            .Select(x => x.Action)
            .ToHashSet();

        Assert.Contains(ActionType.CreateUser, actions);
        Assert.Contains(ActionType.EditUser, actions);
        Assert.DoesNotContain(ActionType.DeleteUser, actions);
        Assert.Equal(2, actions.Count);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveAllPermissions_When_EmptyPermissionsCollectionIsProvided()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.EditUserGroup);
        var userGroup = CreateUserGroup();

        userGroup.AddPermission(ActionType.CreateUser);
        userGroup.AddPermission(ActionType.DeleteUser);

        var command = new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateModel(
                Permissions: Array.Empty<UserGroupPermissionUpdateModel>()));

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Empty(userGroup.Permissions);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PreservePermissions_When_PermissionsIsNull()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.EditUserGroup);
        var userGroup = CreateUserGroup();

        userGroup.AddPermission(ActionType.CreateUser);
        userGroup.AddPermission(ActionType.DeleteUser);

        var originalActions = userGroup.Permissions
            .Select(x => x.Action)
            .ToHashSet();

        var command = new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateModel(
                Nameid: Nameid.FromString("group.updated"),
                Permissions: null));

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var resultingActions = userGroup.Permissions
            .Select(x => x.Action)
            .ToHashSet();

        Assert.Equal(originalActions, resultingActions);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    private static UserGroupUpdateCommand CreateCommand(
        Guid? userGroupGuid = null,
        UserGroupUpdateModel? model = null)
    {
        return new UserGroupUpdateCommand(
            userGroupGuid ?? Guid.NewGuid(),
            model ?? new UserGroupUpdateModel(
                Nameid: Nameid.FromString("group.updated"),
                Description: Description.FromString("Updated description"),
                IsActive: true,
                Permissions:
                [
                    new UserGroupPermissionUpdateModel(ActionType.CreateUser),
                    new UserGroupPermissionUpdateModel(ActionType.EditUser)
                ]));
    }

    private static User CreateActor()
    {
        return new User
        {
            Nameid = Nameid.FromString("actor"),
            PasswordHash = new PasswordHash("hashedpasswordkfasdlfasjdflasdflasdhflçasdfjasdlfasjdflsadlkasdfjasldfjaslf"),
            DefaultPasswordExpirationPeriod = TimeSpan.FromDays(30),
            Description = Description.Empty,
            IsActive = true
        };
    }

    private static User CreateActorWithPermissions(params ActionType[] actions)
    {
        var actor = CreateActor();

        foreach (var action in actions.Distinct())
        {
            actor.AddPermission(action);
        }

        return actor;
    }

    private static UserGroup CreateUserGroup()
    {
        return new UserGroup
        {
            Nameid = Nameid.FromString("group.original"),
            Description = Description.FromString("Original description"),
            IsActive = true
        };
    }
}