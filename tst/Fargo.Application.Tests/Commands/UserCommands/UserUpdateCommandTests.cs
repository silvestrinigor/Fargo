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
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
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
    public async Task Handle_Should_ThrowInvalidPasswordFargoApplicationException_When_CurrentPasswordIsInvalid()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var passwordUpdate = new UserPasswordUpdateModel(
            new Password("Wrong@123"),
            new Password("NewSecure@123"));

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(Password: passwordUpdate));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Verify(targetUser.PasswordHash, passwordUpdate.CurrentPassword)
            .Returns(false);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<InvalidPasswordFargoApplicationException>(act);

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdatePassword_When_CurrentPasswordIsValid()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();
        var originalPasswordHash = targetUser.PasswordHash;

        var passwordUpdate = new UserPasswordUpdateModel(
                new Password("Current@123"),
                new Password("NewSecure@123"));
        var newPasswordHash = CreatePasswordHash('n');

        var command = CreateCommand(
                targetUser.Guid,
                new UserUpdateModel(Password: passwordUpdate));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Verify(originalPasswordHash, passwordUpdate.CurrentPassword)
            .Returns(true);

        passwordHasher
            .Hash(passwordUpdate.NewPassword)
            .Returns(newPasswordHash);

        // Act
        await handler.Handle(command);

        // Assert
        passwordHasher.Received(1)
            .Verify(originalPasswordHash, passwordUpdate.CurrentPassword);

        passwordHasher.Received(1)
            .Hash(passwordUpdate.NewPassword);

        Assert.Equal(newPasswordHash, targetUser.PasswordHash);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateNameidDescriptionAndPassword_When_AllAreProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditUser);
        var targetUser = CreateUser();

        var newNameid = new Nameid("updated-user");
        var newDescription = new Description("Updated user description.");
        var passwordUpdate = new UserPasswordUpdateModel(
            new Password("Current@123"),
            new Password("NewSecure@123"));
        var newPasswordHash = CreatePasswordHash('z');

        var command = CreateCommand(
            targetUser.Guid,
            new UserUpdateModel(
                Nameid: newNameid,
                Description: newDescription,
                Password: passwordUpdate));

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        passwordHasher
            .Verify(targetUser.PasswordHash, passwordUpdate.CurrentPassword)
            .Returns(true);

        passwordHasher
            .Hash(passwordUpdate.NewPassword)
            .Returns(newPasswordHash);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newNameid, targetUser.Nameid);
        Assert.Equal(newDescription, targetUser.Description);
        Assert.Equal(newPasswordHash, targetUser.PasswordHash);

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

    private static PasswordHash CreatePasswordHash(char value)
        => new(new string(value, PasswordHash.MinLength));
}