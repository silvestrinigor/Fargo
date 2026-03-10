using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
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

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(
            () => handler.Handle(command));

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        userRepository.DidNotReceive()
            .Add(Arg.Any<User>());
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
    public async Task Handle_ShouldAddAllPermissionsFromCommandToCreatedUser()
    {
        // Arrange
        var actor = CreateActorWithCreateUserPermission();
        var passwordHash = CreatePasswordHash('p');
        var permissions = new[]
        {
            ActionType.CreateArticle,
            ActionType.CreateArticle,
            ActionType.DeleteItem
        };

        var command = CreateCommand(
            permissions: permissions);

        User? addedUser = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureAnyPasswordHash(passwordHash);
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
        var command = CreateCommand(permissions: null);
        User? addedUser = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureAnyPasswordHash(passwordHash);
        CaptureAddedUser(user => addedUser = user);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedUser);
        Assert.Empty(addedUser!.UserPermissions);
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
                Permissions: permissions));
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

    private static PasswordHash CreatePasswordHash(char value)
        => new(new string(value, PasswordHash.MinLength));
}