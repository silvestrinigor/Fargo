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
        var actorGuid = Guid.NewGuid();
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository
            .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        Task act() => handler.Handle(command);

        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await userRepository.DidNotReceive()
            .GetByGuid(command.UserGuid, Arg.Any<CancellationToken>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHaveEditUserPermission()
    {
        var actor = CreateUser();
        var targetUser = CreateUser();
        var command = CreateCommand(targetUser.Guid);

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        Task act() => handler.Handle(command);

        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotFoundFargoApplicationException_When_TargetUserIsNotFound()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUserGuid = Guid.NewGuid();
        var command = CreateCommand(targetUserGuid);

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        userRepository
            .GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        Task act() => handler.Handle(command);

        await Assert.ThrowsAsync<UserNotFoundFargoApplicationException>(act);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateNameid_When_NameidIsProvided()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newNameid = new Nameid("updated-user");

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(nameid: newNameid));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(newNameid, targetUser.Nameid);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateDescription_When_DescriptionIsProvided()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newDescription = new Description("Updated user description.");

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(description: newDescription));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(newDescription, targetUser.Description);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateDefaultPasswordExpirationTimeSpan_When_ValueIsProvided()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newDefaultPasswordExpirationTimeSpan = TimeSpan.FromDays(15);

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(
                defaultPasswordExpirationTimeSpan: newDefaultPasswordExpirationTimeSpan));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(
            newDefaultPasswordExpirationTimeSpan,
            targetUser.DefaultPasswordExpirationPeriod);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotUpdateDefaultPasswordExpirationTimeSpan_When_ValueIsNotProvided()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var originalDefaultPasswordExpirationTimeSpan =
            targetUser.DefaultPasswordExpirationPeriod;

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel());

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(
            originalDefaultPasswordExpirationTimeSpan,
            targetUser.DefaultPasswordExpirationPeriod);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorTriesToChangeOtherUserPasswordWithoutPermission()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newPassword = new Password("NewSecure@123");

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(password: newPassword));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        Task act() => handler.Handle(command);

        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdatePassword_When_ActorHasChangeOtherUserPasswordPermission()
    {
        var actor = CreateUserWithPermissions(
            ActionType.EditUser,
            ActionType.ChangeOtherUserPassword);

        var targetUser = CreateUser();
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('n');

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(password: newPassword));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        await handler.Handle(command);

        passwordHasher.Received(1)
            .Hash(newPassword);

        Assert.Equal(newPasswordHash, targetUser.PasswordHash);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AddPermissions_When_TheyDoNotExist()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(
                permissions:
                [
                    new UserPermissionUpdateModel(ActionType.CreateUser),
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        var actions = targetUser.Permissions
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
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        targetUser.AddPermission(ActionType.CreateUser);
        targetUser.AddPermission(ActionType.EditUser);
        targetUser.AddPermission(ActionType.DeleteUser);

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(
                permissions:
                [
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        var actions = targetUser.Permissions
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
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        targetUser.AddPermission(ActionType.CreateUser);
        targetUser.AddPermission(ActionType.DeleteUser);

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(
                permissions:
                [
                    new UserPermissionUpdateModel(ActionType.CreateUser),
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        var actions = targetUser.Permissions
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
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(
                permissions:
                [
                    new UserPermissionUpdateModel(ActionType.EditUser),
                    new UserPermissionUpdateModel(ActionType.EditUser),
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        var editUserCount = targetUser.Permissions
            .Count(x => x.Action == ActionType.EditUser);

        Assert.Equal(1, editUserCount);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateNameidFirstNameLastNameDescriptionPasswordAndDefaultPasswordExpirationTimeSpan_When_AllAreProvided()
    {
        var actor = CreateUserWithPermissions(
            ActionType.EditUser,
            ActionType.ChangeOtherUserPassword);

        var targetUser = CreateUser();

        var newNameid = new Nameid("updated-user");
        var newFirstName = new FirstName("Igor");
        var newLastName = new LastName("Silvestrin");
        var newDescription = new Description("Updated user description.");
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('z');
        var newDefaultPasswordExpirationTimeSpan = TimeSpan.FromDays(45);

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(
                nameid: newNameid,
                firstName: newFirstName,
                lastName: newLastName,
                description: newDescription,
                password: newPassword,
                defaultPasswordExpirationTimeSpan: newDefaultPasswordExpirationTimeSpan));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        await handler.Handle(command);

        Assert.Equal(newNameid, targetUser.Nameid);
        Assert.Equal(newFirstName, targetUser.FirstName);
        Assert.Equal(newLastName, targetUser.LastName);
        Assert.Equal(newDescription, targetUser.Description);
        Assert.Equal(newPasswordHash, targetUser.PasswordHash);
        Assert.Equal(
            newDefaultPasswordExpirationTimeSpan,
            targetUser.DefaultPasswordExpirationPeriod);

        passwordHasher.Received(1)
            .Hash(newPassword);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotChangeUser_When_UpdateModelHasNoValues()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        var originalNameid = targetUser.Nameid;
        var originalFirstName = targetUser.FirstName;
        var originalLastName = targetUser.LastName;
        var originalDescription = targetUser.Description;
        var originalPasswordHash = targetUser.PasswordHash;
        var originalDefaultPasswordExpirationTimeSpan =
            targetUser.DefaultPasswordExpirationPeriod;
        var originalActions = targetUser.Permissions
            .Select(x => x.Action)
            .ToHashSet();

        var command = CreateCommand(targetUser.Guid, CreateUpdateModel());

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(originalNameid, targetUser.Nameid);
        Assert.Equal(originalFirstName, targetUser.FirstName);
        Assert.Equal(originalLastName, targetUser.LastName);
        Assert.Equal(originalDescription, targetUser.Description);
        Assert.Equal(originalPasswordHash, targetUser.PasswordHash);
        Assert.Equal(
            originalDefaultPasswordExpirationTimeSpan,
            targetUser.DefaultPasswordExpirationPeriod);
        Assert.Equal(
            originalActions,
            targetUser.Permissions.Select(x => x.Action).ToHashSet());

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
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

        await handler.Handle(command, cancellationToken);

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
        var actor = CreateUserWithPermission(ActionType.EditUser);

        var command = CreateCommand(
            actor.Guid,
            CreateUpdateModel(
                permissions:
                [
                    new UserPermissionUpdateModel(ActionType.CreateUser),
                    new UserPermissionUpdateModel(ActionType.DeleteUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        Task act() => handler.Handle(command);

        var exception =
            await Assert.ThrowsAsync<UserCannotChangeOwnPermissionsFargoDomainException>(act);

        Assert.Equal(actor.Guid, exception.UserGuid);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotAddPermissions_When_ActorTriesToChangeOwnPermissions()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var originalActions = actor.Permissions
            .Select(x => x.Action)
            .ToHashSet();

        var command = CreateCommand(
            actor.Guid,
            CreateUpdateModel(
                permissions:
                [
                    new UserPermissionUpdateModel(ActionType.CreateUser),
                    new UserPermissionUpdateModel(ActionType.DeleteUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        Task act() => handler.Handle(command);

        await Assert.ThrowsAsync<UserCannotChangeOwnPermissionsFargoDomainException>(act);

        Assert.Equal(
            originalActions,
            actor.Permissions.Select(x => x.Action).ToHashSet());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotRemovePermissions_When_ActorTriesToChangeOwnPermissions()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        actor.AddPermission(ActionType.CreateUser);
        actor.AddPermission(ActionType.DeleteUser);

        var originalActions = actor.Permissions
            .Select(x => x.Action)
            .ToHashSet();

        var command = CreateCommand(
            actor.Guid,
            CreateUpdateModel(
                permissions:
                [
                    new UserPermissionUpdateModel(ActionType.EditUser)
                ]));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        Task act() => handler.Handle(command);

        await Assert.ThrowsAsync<UserCannotChangeOwnPermissionsFargoDomainException>(act);

        Assert.Equal(
            originalActions,
            actor.Permissions.Select(x => x.Action).ToHashSet());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RequirePasswordChange_When_AdminChangesUserPassword()
    {
        var actor = CreateUserWithPermissions(
            ActionType.EditUser,
            ActionType.ChangeOtherUserPassword);

        var targetUser = CreateUser();
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('n');

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(password: newPassword));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        await handler.Handle(command);

        Assert.Equal(newPasswordHash, targetUser.PasswordHash);
        Assert.True(targetUser.IsPasswordChangeRequired);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateFirstName_When_FirstNameIsProvided()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newFirstName = new FirstName("Igor");

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(firstName: newFirstName));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(newFirstName, targetUser.FirstName);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateLastName_When_LastNameIsProvided()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var newLastName = new LastName("Silvestrin");

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(lastName: newLastName));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(newLastName, targetUser.LastName);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateFirstNameAndLastName_When_BothAreProvided()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        var newFirstName = new FirstName("Igor");
        var newLastName = new LastName("Silvestrin");

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(
                firstName: newFirstName,
                lastName: newLastName));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(newFirstName, targetUser.FirstName);
        Assert.Equal(newLastName, targetUser.LastName);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotChangeFirstNameAndLastName_When_TheyAreNotProvided()
    {
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        targetUser.FirstName = new FirstName("Original");
        targetUser.LastName = new LastName("User");

        var originalFirstName = targetUser.FirstName;
        var originalLastName = targetUser.LastName;

        var command = CreateCommand(
            targetUser.Guid,
            CreateUpdateModel(description: new Description("Updated description.")));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        await handler.Handle(command);

        Assert.Equal(originalFirstName, targetUser.FirstName);
        Assert.Equal(originalLastName, targetUser.LastName);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_SetIsActiveToFalse_When_ValueIsProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var user = CreateUser();
        user.Deactivate();

        var command = CreateCommand(
                userGuid: user.Guid,
                model: new UserUpdateModel(IsActive: false));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(user);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.False(user.IsActive);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_SetIsActiveToTrue_When_ValueIsProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var user = CreateUser();
        user.Deactivate();

        var command = CreateCommand(
                userGuid: user.Guid,
                model: new UserUpdateModel(IsActive: true));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(user);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.True(user.IsActive);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_KeepIsActiveUnchanged_When_ValueIsNotProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var user = CreateUser();
        user.Deactivate();

        var command = CreateCommand(
                userGuid: user.Guid,
                model: new UserUpdateModel());

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(user);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.False(user.IsActive);

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
                model ?? CreateUpdateModel());

    private static UserUpdateModel CreateUpdateModel(
            Nameid? nameid = null,
            FirstName? firstName = null,
            LastName? lastName = null,
            Description? description = null,
            Password? password = null,
            IReadOnlyCollection<UserPermissionUpdateModel>? permissions = null,
            TimeSpan? defaultPasswordExpirationTimeSpan = null)
    {
        return new UserUpdateModel(
                Nameid: nameid,
                FirstName: firstName,
                LastName: lastName,
                Description: description,
                Password: password,
                Permissions: permissions,
                DefaultPasswordExpirationPeriod: defaultPasswordExpirationTimeSpan);
    }

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("user123"),
            FirstName = new FirstName("Default"),
            LastName = new LastName("User"),
            Description = new Description("Original description."),
            PasswordHash = CreatePasswordHash('a'),
            DefaultPasswordExpirationPeriod = TimeSpan.FromDays(User.DefaultPasswordChangeDays)
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