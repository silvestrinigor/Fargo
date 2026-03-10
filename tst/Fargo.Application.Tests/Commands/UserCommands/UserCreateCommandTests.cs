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
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.UserCommands;

public sealed class UserCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();

    private readonly UserService userService;
    private readonly UserCreateCommandHandler handler;

    public UserCreateCommandHandlerTests()
    {
        userService = new UserService(userRepository);

        handler = new UserCreateCommandHandler(
            userService,
            userRepository,
            unitOfWork,
            currentUser,
            passwordHasher);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessFargoApplicationException_WhenActorIsNotFound()
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

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        userRepository.DidNotReceive()
            .Add(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUserNotAuthorizedFargoDomainException_WhenActorDoesNotHaveCreateUserPermission()
    {
        // Arrange
        var actor = CreateActorWithoutPermissions();
        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        userRepository.DidNotReceive()
            .Add(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowUserNameidAlreadyExistsDomainException_WhenNameidAlreadyExists()
    {
        // Arrange
        var actor = CreateActorWithCreateUserPermission();
        var nameid = new Nameid("existing-user");
        var password = new Password("Secure@123456");
        var passwordHash = CreatePasswordHash('h');
        var command = CreateCommand(nameid, password);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigurePasswordHash(password, passwordHash);

        userRepository
            .ExistsByNameid(nameid, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNameidAlreadyExistsDomainException>(act);

        userRepository.DidNotReceive()
            .Add(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_AddUser_SaveChanges_AndReturnUserGuid_WhenCommandIsValid()
    {
        // Arrange
        var actor = CreateActorWithCreateUserPermission();
        var nameid = new Nameid("user123");
        var password = new Password("asjfasl#123456");
        var passwordHash = CreatePasswordHash('h');
        var command = CreateCommand(nameid, password);
        User? addedUser = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigurePasswordHash(password, passwordHash);
        ConfigureNameidDoesNotExist(nameid);
        CaptureAddedUser(user => addedUser = user);

        // Act
        var result = await handler.Handle(command);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>());

        passwordHasher.Received(1)
            .Hash(password);

        userRepository.Received(1)
            .Add(Arg.Any<User>());

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedUser);
        Assert.Equal(nameid, addedUser!.Nameid);
        Assert.Equal(passwordHash, addedUser.PasswordHash);
        Assert.Equal(addedUser.Guid, result);
    }

    [Fact]
    public async Task Handle_ShouldRequirePasswordChange_WhenUserIsCreated()
    {
        // Arrange
        var actor = CreateActorWithCreateUserPermission();
        var nameid = new Nameid("user123");
        var password = new Password("Secure@123456");
        var passwordHash = CreatePasswordHash('h');
        var command = CreateCommand(nameid, password);
        User? addedUser = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigurePasswordHash(password, passwordHash);
        ConfigureNameidDoesNotExist(nameid);
        CaptureAddedUser(user => addedUser = user);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedUser);
        Assert.True(addedUser!.IsPasswordChangeRequired);
    }

    [Fact]
    public async Task Handle_ShouldAddAllPermissionsFromCommandToCreatedUser()
    {
        // Arrange
        var actor = CreateActorWithCreateUserPermission();
        var passwordHash = CreatePasswordHash('p');
        var commandNameid = new Nameid("user123");
        var permissions = new[]
        {
            ActionType.CreateArticle,
            ActionType.CreateArticle,
            ActionType.DeleteItem
        };

        var command = CreateCommand(
            nameid: commandNameid,
            permissions: permissions);

        User? addedUser = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureAnyPasswordHash(passwordHash);
        ConfigureNameidDoesNotExist(commandNameid);
        CaptureAddedUser(user => addedUser = user);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedUser);

        Assert.Contains(
            addedUser!.UserPermissions,
            x => x.Action == ActionType.CreateArticle);

        Assert.Contains(
            addedUser.UserPermissions,
            x => x.Action == ActionType.DeleteItem);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserWithoutPermissions_WhenCommandPermissionsIsNull()
    {
        // Arrange
        var actor = CreateActorWithCreateUserPermission();
        var passwordHash = CreatePasswordHash('p');
        var commandNameid = new Nameid("user123");
        var command = CreateCommand(nameid: commandNameid, permissions: null);
        User? addedUser = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureAnyPasswordHash(passwordHash);
        ConfigureNameidDoesNotExist(commandNameid);
        CaptureAddedUser(user => addedUser = user);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedUser);
        Assert.Empty(addedUser!.UserPermissions);
    }

    [Fact]
    public async Task Handle_ShouldUseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateActorWithCreateUserPermission();
        var nameid = new Nameid("user123");
        var password = new Password("Secure@123456");
        var passwordHash = CreatePasswordHash('h');
        var cancellationToken = new CancellationTokenSource().Token;
        var command = CreateCommand(nameid, password);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        userRepository
            .ExistsByNameid(nameid, cancellationToken)
            .Returns(false);

        passwordHasher
            .Hash(password)
            .Returns(passwordHash);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, cancellationToken);

        await userRepository.Received(1)
            .ExistsByNameid(nameid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    private void ConfigureCurrentUser(User actor)
    {
        currentUser.UserGuid.Returns(actor.Guid);
    }

    private void ConfigureActorLookup(User actor)
    {
        userRepository
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);
    }

    private void ConfigurePasswordHash(Password password, PasswordHash passwordHash)
    {
        passwordHasher
            .Hash(password)
            .Returns(passwordHash);
    }

    private void ConfigureAnyPasswordHash(PasswordHash passwordHash)
    {
        passwordHasher
            .Hash(Arg.Any<Password>())
            .Returns(passwordHash);
    }

    private void ConfigureNameidDoesNotExist(Nameid nameid)
    {
        userRepository
            .ExistsByNameid(nameid, Arg.Any<CancellationToken>())
            .Returns(false);
    }

    private void CaptureAddedUser(Action<User> capture)
    {
        userRepository
            .When(x => x.Add(Arg.Any<User>()))
            .Do(callInfo => capture(callInfo.Arg<User>()));
    }

    private static UserCreateCommand CreateCommand(
        Nameid? nameid = null,
        Password? password = null,
        IReadOnlyCollection<ActionType>? permissions = null)
    {
        return new UserCreateCommand(
            new UserCreateModel(
                nameid ?? new Nameid("user123"),
                password ?? new Password("Secure@123456"),
                Permissions:
                    permissions is not null
                    ? [.. permissions.Select(x => new UserPermissionUpdateModel(x))]
                    : null
            ));
    }

    private static User CreateActorWithCreateUserPermission()
    {
        var actor = new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("admin-user"),
            PasswordHash = CreatePasswordHash('a')
        };

        actor.AddPermission(ActionType.CreateUser);
        return actor;
    }

    private static User CreateActorWithoutPermissions()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("basic-user"),
            PasswordHash = CreatePasswordHash('a')
        };
    }

    private static PasswordHash CreatePasswordHash(char value)
        => new(new string(value, PasswordHash.MinLength));
}