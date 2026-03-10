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
    [Fact]
    public async Task Handle_Should_DoNothing_When_AnyUserAlreadyExists()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var defaultAdminOptions = Options.Create(new DefaultAdminOptions
        {
            Nameid = "admin",
            Password = "Secure@123"
        });

        var handler = new InitializeSystemCommandHandler(
            userRepository,
            passwordHasher,
            unitOfWork,
            defaultAdminOptions);

        var command = new InitializeSystemCommand();

        userRepository
            .Any(Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await handler.Handle(command);

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
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var defaultAdminOptions = Options.Create(new DefaultAdminOptions
        {
            Nameid = "admin",
            Password = "Secure@123"
        });

        var handler = new InitializeSystemCommandHandler(
            userRepository,
            passwordHasher,
            unitOfWork,
            defaultAdminOptions);

        var expectedNameid = new Nameid("admin");
        var expectedPassword = new Password("Secure@123");
        var passwordHash = new PasswordHash(new string('p', PasswordHash.MinLength));

        var command = new InitializeSystemCommand();

        User? addedUser = null;

        userRepository
            .Any(Arg.Any<CancellationToken>())
            .Returns(false);

        passwordHasher
            .Hash(expectedPassword)
            .Returns(passwordHash);

        userRepository
            .When(x => x.Add(Arg.Any<User>()))
            .Do(callInfo => addedUser = callInfo.Arg<User>());

        // Act
        await handler.Handle(command);

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
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var defaultAdminOptions = Options.Create(new DefaultAdminOptions
        {
            Nameid = "admin",
            Password = "Secure@123"
        });

        var handler = new InitializeSystemCommandHandler(
            userRepository,
            passwordHasher,
            unitOfWork,
            defaultAdminOptions);

        var expectedPassword = new Password("Secure@123");
        var passwordHash = new PasswordHash(new string('p', PasswordHash.MinLength));

        var command = new InitializeSystemCommand();

        User? addedUser = null;

        userRepository
            .Any(Arg.Any<CancellationToken>())
            .Returns(false);

        passwordHasher
            .Hash(expectedPassword)
            .Returns(passwordHash);

        userRepository
            .When(x => x.Add(Arg.Any<User>()))
            .Do(callInfo => addedUser = callInfo.Arg<User>());

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedUser);

        var actions = Enum.GetValues<ActionType>();

        Assert.Equal(actions.Length, addedUser!.UserPermissions.Count);

        foreach (var action in actions)
        {
            Assert.Contains(addedUser.UserPermissions, x => x.Action == action);
        }
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var defaultAdminOptions = Options.Create(new DefaultAdminOptions
        {
            Nameid = "admin",
            Password = "Secure@123"
        });

        var handler = new InitializeSystemCommandHandler(
            userRepository,
            passwordHasher,
            unitOfWork,
            defaultAdminOptions);

        var expectedPassword = new Password("Secure@123");
        var passwordHash = new PasswordHash(new string('p', PasswordHash.MinLength));
        var cancellationToken = new CancellationTokenSource().Token;

        var command = new InitializeSystemCommand();

        userRepository
            .Any(cancellationToken)
            .Returns(false);

        passwordHasher
            .Hash(expectedPassword)
            .Returns(passwordHash);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .Any(cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }
}