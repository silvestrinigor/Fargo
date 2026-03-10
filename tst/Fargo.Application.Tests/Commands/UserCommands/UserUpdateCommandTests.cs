using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.UserCommands;

public sealed class UserUpdateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly UserUpdateCommandHandler handler;

    public UserUpdateCommandHandlerTests()
    {
        handler = new UserUpdateCommandHandler(
            userRepository,
            passwordHasher,
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

        userRepository
            .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await userRepository.DidNotReceive()
            .GetByGuid(command.UserGuid, Arg.Any<CancellationToken>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHaveEditUserPermission()
    {
        // Arrange
        var actor = CreateUser();
        var targetUser = CreateUser();
        var command = CreateCommand(targetUser.Guid);

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotFoundFargoApplicationException_When_TargetUserIsNotFound()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUserGuid = Guid.NewGuid();
        var command = CreateCommand(targetUserGuid);

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        userRepository
            .GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotFoundFargoApplicationException>(act);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateNameid_When_NameidIsProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newNameid = new Nameid("updated-user");

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(Nameid: newNameid));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newNameid, targetUser.Nameid);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateDescription_When_DescriptionIsProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newDescription = new Description("Updated user description.");

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(Description: newDescription));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newDescription, targetUser.Description);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateDefaultPasswordExpirationTimeSpan_When_ValueIsProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newDefaultPasswordExpirationTimeSpan = TimeSpan.FromDays(15);

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(
                DefaultPasswordExpirationTimeSpan: newDefaultPasswordExpirationTimeSpan));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(
            newDefaultPasswordExpirationTimeSpan,
            targetUser.DefaultPasswordExpirationTimeSpan);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDefaultPasswordExpirationTimeSpan_When_ValueIsNotProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var originalDefaultPasswordExpirationTimeSpan =
            targetUser.DefaultPasswordExpirationTimeSpan;

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel());

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(
            originalDefaultPasswordExpirationTimeSpan,
            targetUser.DefaultPasswordExpirationTimeSpan);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorTriesToChangeOtherUserPasswordWithoutPermission()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newPassword = new Password("NewSecure@123");

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(Password: newPassword));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdatePassword_When_ActorHasChangeOtherUserPasswordPermission()
    {
        // Arrange
        var actor = CreateUserWithPermissions(
            ActionType.EditUser,
            ActionType.ChangeOtherUserPassword);

        var targetUser = CreateUser();
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('n');

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(Password: newPassword));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        // Act
        await handler.Handle(command);

        // Assert
        passwordHasher.Received(1)
            .Hash(newPassword);

        Assert.Equal(newPasswordHash, targetUser.PasswordHash);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AddPermissions_When_TheyDoNotExist()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(
                Permissions:
                [
                    new UserPermissionUpdateModel(ActionType.CreateUser),
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        var actions = targetUser.UserPermissions
            .Select(x => x.Action)
            .ToHashSet();

        Assert.Contains(ActionType.CreateUser, actions);
        Assert.Contains(ActionType.EditUser, actions);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemovePermissions_ThatAreNotRequested()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        targetUser.AddPermission(ActionType.CreateUser);
        targetUser.AddPermission(ActionType.EditUser);
        targetUser.AddPermission(ActionType.DeleteUser);

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(
                Permissions:
                [
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        var actions = targetUser.UserPermissions
            .Select(x => x.Action)
            .ToHashSet();

        Assert.Contains(ActionType.EditUser, actions);
        Assert.DoesNotContain(ActionType.CreateUser, actions);
        Assert.DoesNotContain(ActionType.DeleteUser, actions);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_SynchronizePermissions_When_AddingAndRemovingAreRequired()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        targetUser.AddPermission(ActionType.CreateUser);
        targetUser.AddPermission(ActionType.DeleteUser);

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(
                Permissions:
                [
                    new UserPermissionUpdateModel(ActionType.CreateUser),
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        var actions = targetUser.UserPermissions
            .Select(x => x.Action)
            .ToHashSet();

        Assert.Contains(ActionType.CreateUser, actions);
        Assert.Contains(ActionType.EditUser, actions);
        Assert.DoesNotContain(ActionType.DeleteUser, actions);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_IgnoreDuplicatePermissions_When_PermissionsContainDuplicates()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(
                Permissions:
                [
                    new UserPermissionUpdateModel(ActionType.EditUser),
                    new UserPermissionUpdateModel(ActionType.EditUser),
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        var editUserCount = targetUser.UserPermissions
            .Count(x => x.Action == ActionType.EditUser);

        Assert.Equal(1, editUserCount);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateNameidDescriptionPasswordAndDefaultPasswordExpirationTimeSpan_When_AllAreProvided()
    {
        // Arrange
        var actor = CreateUserWithPermissions(
            ActionType.EditUser,
            ActionType.ChangeOtherUserPassword);

        var targetUser = CreateUser();

        var newNameid = new Nameid("updated-user");
        var newDescription = new Description("Updated user description.");
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('z');
        var newDefaultPasswordExpirationTimeSpan = TimeSpan.FromDays(45);

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(
                Nameid: newNameid,
                Description: newDescription,
                Password: newPassword,
                DefaultPasswordExpirationTimeSpan: newDefaultPasswordExpirationTimeSpan));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newNameid, targetUser.Nameid);
        Assert.Equal(newDescription, targetUser.Description);
        Assert.Equal(newPasswordHash, targetUser.PasswordHash);
        Assert.Equal(
            newDefaultPasswordExpirationTimeSpan,
            targetUser.DefaultPasswordExpirationTimeSpan);

        passwordHasher.Received(1)
            .Hash(newPassword);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotChangeUser_When_UpdateModelHasNoValues()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        var originalNameid = targetUser.Nameid;
        var originalDescription = targetUser.Description;
        var originalPasswordHash = targetUser.PasswordHash;
        var originalDefaultPasswordExpirationTimeSpan =
            targetUser.DefaultPasswordExpirationTimeSpan;
        var originalActions = targetUser.UserPermissions
            .Select(x => x.Action)
            .ToHashSet();

        var command = CreateCommand(targetUser.Guid, new UserUpdateModel());

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(originalNameid, targetUser.Nameid);
        Assert.Equal(originalDescription, targetUser.Description);
        Assert.Equal(originalPasswordHash, targetUser.PasswordHash);
        Assert.Equal(
            originalDefaultPasswordExpirationTimeSpan,
            targetUser.DefaultPasswordExpirationTimeSpan);
        Assert.Equal(
            originalActions,
            targetUser.UserPermissions.Select(x => x.Action).ToHashSet());

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var cancellationToken = new CancellationTokenSource().Token;
        var command = CreateCommand(targetUser.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        userRepository
            .GetByGuid(targetUser.Guid, cancellationToken)
            .Returns(targetUser);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, cancellationToken);

        await userRepository.Received(1)
            .GetByGuid(targetUser.Guid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ThrowUserCannotChangeOwnPermissionsFargoDomainException_When_ActorTriesToChangeOwnPermissions()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);

        var command = CreateCommand(
            actor.Guid,
            new UserUpdateModel(
                Permissions:
                [
                    new UserPermissionUpdateModel(ActionType.CreateUser),
                    new UserPermissionUpdateModel(ActionType.DeleteUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        var exception =
            await Assert.ThrowsAsync<UserCannotChangeOwnPermissionsFargoDomainException>(act);

        Assert.Equal(actor.Guid, exception.UserGuid);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotAddPermissions_When_ActorTriesToChangeOwnPermissions()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var originalActions = actor.UserPermissions
            .Select(x => x.Action)
            .ToHashSet();

        var command = CreateCommand(
            actor.Guid,
            new UserUpdateModel(
                Permissions:
                [
                    new UserPermissionUpdateModel(ActionType.CreateUser),
                    new UserPermissionUpdateModel(ActionType.DeleteUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserCannotChangeOwnPermissionsFargoDomainException>(act);

        Assert.Equal(
            originalActions,
            actor.UserPermissions.Select(x => x.Action).ToHashSet());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotRemovePermissions_When_ActorTriesToChangeOwnPermissions()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        actor.AddPermission(ActionType.CreateUser);
        actor.AddPermission(ActionType.DeleteUser);

        var originalActions = actor.UserPermissions
            .Select(x => x.Action)
            .ToHashSet();

        var command = CreateCommand(
            actor.Guid,
            new UserUpdateModel(
                Permissions:
                [
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserCannotChangeOwnPermissionsFargoDomainException>(act);

        Assert.Equal(
            originalActions,
            actor.UserPermissions.Select(x => x.Action).ToHashSet());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RequirePasswordChange_When_AdminChangesUserPassword()
    {
        // Arrange
        var actor = CreateUserWithPermissions(
                ActionType.EditUser,
                ActionType.ChangeOtherUserPassword);

        var targetUser = CreateUser();
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('n');

        var command = CreateCommand(
                targetUser.Guid,
                new UserUpdateModel(Password: newPassword));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newPasswordHash, targetUser.PasswordHash);
        Assert.True(targetUser.IsPasswordChangeRequired);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    private void ConfigureCurrentUser(User actor)
    {
        currentUser.UserGuid.Returns(actor.Guid);
    }

    private void ConfigureUserLookup(User user)
    {
        userRepository
            .GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);
    }

    private static UserUpdateCommand CreateCommand(
            Guid? userGuid = null,
            UserUpdateModel? model = null)
        => new(
                userGuid ?? Guid.NewGuid(),
                model ?? new UserUpdateModel());

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("user123"),
            Description = new Description("Original description."),
            PasswordHash = CreatePasswordHash('a')
        };
    }

    private static User CreateUserWithPermission(ActionType action)
    {
        var user = CreateUser();
        user.AddPermission(action);
        return user;
    }

    private static User CreateUserWithPermissions(params ActionType[] actions)
    {
        var user = CreateUser();

        foreach (var action in actions)
        {
            user.AddPermission(action);
        }

        return user;
    }

    private static PasswordHash CreatePasswordHash(char value)
        => new(new string(value, PasswordHash.MinLength));
}