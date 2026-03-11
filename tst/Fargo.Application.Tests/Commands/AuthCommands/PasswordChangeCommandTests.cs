using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.AuthCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.AuthCommands;

public sealed class PasswordChangeCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly PasswordChangeCommandHandler handler;

    public PasswordChangeCommandHandlerTests()
    {
        handler = new PasswordChangeCommandHandler(
                userRepository,
                passwordHasher,
                unitOfWork,
                currentUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_CurrentUserIsNotFound()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var command = CreateCommand();

        currentUser.UserGuid.Returns(userGuid);

        userRepository
            .GetByGuid(userGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        passwordHasher.DidNotReceive()
            .Verify(Arg.Any<PasswordHash>(), Arg.Any<Password>());

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidPasswordFargoApplicationException_When_CurrentPasswordIsNull()
    {
        // Arrange
        var user = CreateUser();
        var command = new PasswordChangeCommand(
                new UserPasswordUpdateModel(
                    NewPassword: new Password("NewSecure@123"),
                    CurrentPassword: null));

        ConfigureCurrentUser(user);
        ConfigureUserLookup(user);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<InvalidPasswordFargoApplicationException>(act);

        passwordHasher.DidNotReceive()
            .Verify(Arg.Any<PasswordHash>(), Arg.Any<Password>());

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidPasswordFargoApplicationException_When_CurrentPasswordIsInvalid()
    {
        // Arrange
        var user = CreateUser();
        var currentPassword = new Password("Wrong@123");
        var newPassword = new Password("NewSecure@123");
        var command = CreateCommand(currentPassword, newPassword);

        ConfigureCurrentUser(user);
        ConfigureUserLookup(user);

        passwordHasher
            .Verify(user.PasswordHash, currentPassword)
            .Returns(false);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<InvalidPasswordFargoApplicationException>(act);

        passwordHasher.Received(1)
            .Verify(user.PasswordHash, currentPassword);

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdatePassword_When_CurrentPasswordIsValid()
    {
        // Arrange
        var user = CreateUser();
        var originalPasswordHash = user.PasswordHash;
        var currentPassword = new Password("Current@123");
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('n');
        var command = CreateCommand(currentPassword, newPassword);

        ConfigureCurrentUser(user);
        ConfigureUserLookup(user);

        passwordHasher
            .Verify(originalPasswordHash, currentPassword)
            .Returns(true);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        // Act
        await handler.Handle(command);

        // Assert
        passwordHasher.Received(1)
            .Verify(originalPasswordHash, currentPassword);

        passwordHasher.Received(1)
            .Hash(newPassword);

        Assert.Equal(newPasswordHash, user.PasswordHash);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ClearPasswordChangeRequirement_When_PasswordIsChangedSuccessfully_And_ExpirationIsPositive()
    {
        // Arrange
        var user = CreateUser();
        user.DefaultPasswordExpirationPeriod = TimeSpan.FromDays(30);
        user.MarkPasswordChangeAsRequired();

        var currentPassword = new Password("Current@123");
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('z');
        var command = CreateCommand(currentPassword, newPassword);

        ConfigureCurrentUser(user);
        ConfigureUserLookup(user);

        passwordHasher
            .Verify(user.PasswordHash, currentPassword)
            .Returns(true);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.False(user.IsPasswordChangeRequired);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var user = CreateUser();
        var currentPassword = new Password("Current@123");
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('x');
        var cancellationToken = new CancellationTokenSource().Token;
        var command = CreateCommand(currentPassword, newPassword);

        currentUser.UserGuid.Returns(user.Guid);

        userRepository
            .GetByGuid(user.Guid, cancellationToken)
            .Returns(user);

        passwordHasher
            .Verify(user.PasswordHash, currentPassword)
            .Returns(true);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(user.Guid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ResetPasswordExpiration_BasedOnUserDefaultPasswordExpirationTimeSpan()
    {
        // Arrange
        var user = CreateUser();
        user.DefaultPasswordExpirationPeriod = TimeSpan.FromDays(15);

        var currentPassword = new Password("Current@123");
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('r');
        var command = CreateCommand(currentPassword, newPassword);

        ConfigureCurrentUser(user);
        ConfigureUserLookup(user);

        passwordHasher
            .Verify(user.PasswordHash, currentPassword)
            .Returns(true);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        var before = DateTimeOffset.UtcNow;

        // Act
        await handler.Handle(command);

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.True(user.RequirePasswordChangeAt >= before.AddDays(15));
        Assert.True(user.RequirePasswordChangeAt <= after.AddDays(15));

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ExpireImmediately_When_DefaultPasswordExpirationTimeSpanIsZero()
    {
        // Arrange
        var user = CreateUser();
        user.DefaultPasswordExpirationPeriod = TimeSpan.Zero;

        var currentPassword = new Password("Current@123");
        var newPassword = new Password("NewSecure@123");
        var newPasswordHash = CreatePasswordHash('q');
        var command = CreateCommand(currentPassword, newPassword);

        ConfigureCurrentUser(user);
        ConfigureUserLookup(user);

        passwordHasher
            .Verify(user.PasswordHash, currentPassword)
            .Returns(true);

        passwordHasher
            .Hash(newPassword)
            .Returns(newPasswordHash);

        var before = DateTimeOffset.UtcNow;

        // Act
        await handler.Handle(command);

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.True(user.RequirePasswordChangeAt >= before);
        Assert.True(user.RequirePasswordChangeAt <= after);
        Assert.True(user.IsPasswordChangeRequired);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_CurrentUserIsInactive()
    {
        // Arrange
        var user = CreateUser(isActive: false);
        var command = CreateCommand();

        ConfigureCurrentUser(user);
        ConfigureUserLookup(user);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        passwordHasher.DidNotReceive()
            .Verify(Arg.Any<PasswordHash>(), Arg.Any<Password>());

        passwordHasher.DidNotReceive()
            .Hash(Arg.Any<Password>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_CurrentUserIsInactive_BeforeValidatingPassword()
    {
        // Arrange
        var user = CreateUser(isActive: false);
        var currentPassword = new Password("Current@123");
        var newPassword = new Password("NewSecure@123");
        var command = CreateCommand(currentPassword, newPassword);

        ConfigureCurrentUser(user);
        ConfigureUserLookup(user);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        passwordHasher.DidNotReceive()
            .Verify(user.PasswordHash, currentPassword);

        passwordHasher.DidNotReceive()
            .Hash(newPassword);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    private void ConfigureCurrentUser(User user)
    {
        currentUser.UserGuid.Returns(user.Guid);
    }

    private void ConfigureUserLookup(User user)
    {
        userRepository
            .GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);
    }

    private static PasswordChangeCommand CreateCommand(
            Password? currentPassword = null,
            Password? newPassword = null)
    {
        return new PasswordChangeCommand(
                new UserPasswordUpdateModel(
                    NewPassword: newPassword ?? new Password("NewSecure@123"),
                    CurrentPassword: currentPassword ?? new Password("Current@123")));
    }

    private static User CreateUser(bool isActive = true)
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("user123"),
            PasswordHash = CreatePasswordHash('a'),
            IsActive = isActive
        };
    }

    private static PasswordHash CreatePasswordHash(char value)
        => new(new string(value, PasswordHash.MinLength));
}