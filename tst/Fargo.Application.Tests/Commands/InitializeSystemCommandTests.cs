using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Fargo.Application.Tests.Commands;

public sealed class InitializeSystemCommandHandlerTests
{
    private static readonly DefaultAdminOptions defaultAdminOptionsValue = new()
    {
        Nameid = "admin",
        Password = "Secure@123"
    };

    private static readonly Nameid expectedNameid = new("admin");
    private static readonly Password expectedPassword = new("Secure@123");

    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOptions<DefaultAdminOptions> defaultAdminOptions =
        Options.Create(defaultAdminOptionsValue);

    private readonly InitializeSystemCommandHandler handler;

    public InitializeSystemCommandHandlerTests()
    {
        handler = new InitializeSystemCommandHandler(
            userRepository,
            passwordHasher,
            unitOfWork,
            defaultAdminOptions);
    }

    [Fact]
    public async Task Handle_Should_DoNothing_When_AnyUserAlreadyExists()
    {
        // Arrange
        userRepository
            .Any(Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await handler.Handle(new InitializeSystemCommand());

        // Assert
        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        userRepository.DidNotReceive()
            .Add(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CreateDefaultAdmin_When_NoUsersExist()
    {
        // Arrange
        var passwordHash = CreatePasswordHash();
        User? addedUser = null;

        ConfigureNoUsersExist();
        ConfigurePasswordHash(passwordHash);
        CaptureAddedUser(user => addedUser = user);

        // Act
        await handler.Handle(new InitializeSystemCommand());

        // Assert
        passwordHasher.Received(1)
            .Hash(expectedPassword);

        userRepository.Received(1)
            .Add(Arg.Any<User>());

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedUser);
        Assert.Equal(expectedNameid, addedUser!.Nameid);
        Assert.Equal(passwordHash, addedUser.PasswordHash);
    }

    [Fact]
    public async Task Handle_Should_AddAllPermissionsToDefaultAdmin_When_NoUsersExist()
    {
        // Arrange
        var passwordHash = CreatePasswordHash();
        User? addedUser = null;

        ConfigureNoUsersExist();
        ConfigurePasswordHash(passwordHash);
        CaptureAddedUser(user => addedUser = user);

        // Act
        await handler.Handle(new InitializeSystemCommand());

        // Assert
        Assert.NotNull(addedUser);

        var actions = Enum.GetValues<ActionType>();

        Assert.Equal(actions.Length, addedUser!.Permissions.Count);

        foreach (var action in actions)
        {
            Assert.Contains(addedUser.Permissions, x => x.Action == action);
        }
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var passwordHash = CreatePasswordHash();

        userRepository
            .Any(cancellationToken)
            .Returns(false);

        ConfigurePasswordHash(passwordHash);

        // Act
        await handler.Handle(new InitializeSystemCommand(), cancellationToken);

        // Assert
        await userRepository.Received(1)
            .Any(cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    private void ConfigureNoUsersExist()
    {
        userRepository
            .Any(Arg.Any<CancellationToken>())
            .Returns(false);
    }

    private void ConfigurePasswordHash(PasswordHash passwordHash)
    {
        passwordHasher
            .Hash(expectedPassword)
            .Returns(passwordHash);
    }

    private void CaptureAddedUser(Action<User> capture)
    {
        userRepository
            .When(x => x.Add(Arg.Any<User>()))
            .Do(callInfo => capture(callInfo.Arg<User>()));
    }

    private static PasswordHash CreatePasswordHash()
        => new(new string('p', PasswordHash.MinLength));
}
