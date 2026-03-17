using Fargo.Application.Exceptions;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.AuthCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.AuthCommands;

public sealed class RefreshCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly ITokenGenerator tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly IRefreshTokenGenerator refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly ITokenHasher tokenHasher = Substitute.For<ITokenHasher>();
    private readonly IRefreshTokenRepository refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly RefreshCommandHandler handler;

    public RefreshCommandHandlerTests()
    {
        handler = new RefreshCommandHandler(
            userRepository,
            tokenGenerator,
            refreshTokenGenerator,
            tokenHasher,
            refreshTokenRepository,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_StoredRefreshTokenIsNotFound()
    {
        // Arrange
        var rawOldRefreshToken = CreateToken('a');
        var oldRefreshTokenHash = CreateTokenHash('b');
        var command = CreateCommand(rawOldRefreshToken);

        ConfigureOldRefreshTokenHash(rawOldRefreshToken, oldRefreshTokenHash);

        refreshTokenRepository
            .GetByTokenHash(oldRefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await userRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        refreshTokenRepository.DidNotReceive()
            .Remove(Arg.Any<RefreshToken>());

        refreshTokenRepository.DidNotReceive()
            .Add(Arg.Any<RefreshToken>());

        tokenGenerator.DidNotReceive()
            .Generate(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_StoredRefreshTokenIsExpired()
    {
        // Arrange
        var rawOldRefreshToken = CreateToken('a');
        var oldRefreshTokenHash = CreateTokenHash('b');
        var storedOldRefreshToken = CreateStoredRefreshToken(
            Guid.NewGuid(),
            oldRefreshTokenHash,
            DateTimeOffset.UtcNow.AddMinutes(-1));
        var command = CreateCommand(rawOldRefreshToken);

        ConfigureOldRefreshTokenHash(rawOldRefreshToken, oldRefreshTokenHash);

        refreshTokenRepository
            .GetByTokenHash(oldRefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns(storedOldRefreshToken);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await userRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        refreshTokenRepository.DidNotReceive()
            .Remove(Arg.Any<RefreshToken>());

        refreshTokenRepository.DidNotReceive()
            .Add(Arg.Any<RefreshToken>());

        tokenGenerator.DidNotReceive()
            .Generate(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_UserIsNotFound()
    {
        // Arrange
        var rawOldRefreshToken = CreateToken('a');
        var oldRefreshTokenHash = CreateTokenHash('b');
        var storedOldRefreshToken = CreateStoredRefreshToken(
            Guid.NewGuid(),
            oldRefreshTokenHash,
            DateTimeOffset.UtcNow.AddMinutes(1));
        var command = CreateCommand(rawOldRefreshToken);

        ConfigureOldRefreshTokenHash(rawOldRefreshToken, oldRefreshTokenHash);

        refreshTokenRepository
            .GetByTokenHash(oldRefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns(storedOldRefreshToken);

        userRepository
            .GetByGuid(storedOldRefreshToken.UserGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        refreshTokenRepository.DidNotReceive()
            .Remove(Arg.Any<RefreshToken>());

        refreshTokenRepository.DidNotReceive()
            .Add(Arg.Any<RefreshToken>());

        tokenGenerator.DidNotReceive()
            .Generate(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnAuthResult_When_RefreshSucceeds()
    {
        // Arrange
        var rawOldRefreshToken = CreateToken('a');
        var oldRefreshTokenHash = CreateTokenHash('b');
        var rawNewRefreshToken = CreateToken('c');
        var newRefreshTokenHash = CreateTokenHash('d');
        var accessToken = CreateToken('e');
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var user = CreateUser();
        var storedOldRefreshToken = CreateStoredRefreshToken(
            user.Guid,
            oldRefreshTokenHash,
            DateTimeOffset.UtcNow.AddMinutes(1));
        var command = CreateCommand(rawOldRefreshToken);

        ConfigureSuccessfulRefresh(
            command,
            user,
            storedOldRefreshToken,
            rawNewRefreshToken,
            newRefreshTokenHash,
            accessToken,
            expiresAt);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(rawNewRefreshToken, result.RefreshToken);
        Assert.Equal(expiresAt, result.ExpiresAt);
    }

    [Fact]
    public async Task Handle_Should_RemoveOldRefreshToken_AddNewRefreshToken_AndSaveChanges_When_RefreshSucceeds()
    {
        // Arrange
        var rawOldRefreshToken = CreateToken('a');
        var oldRefreshTokenHash = CreateTokenHash('b');
        var rawNewRefreshToken = CreateToken('c');
        var newRefreshTokenHash = CreateTokenHash('d');
        var accessToken = CreateToken('e');
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var user = CreateUser();
        var storedOldRefreshToken = CreateStoredRefreshToken(
            user.Guid,
            oldRefreshTokenHash,
            DateTimeOffset.UtcNow.AddMinutes(1));
        var command = CreateCommand(rawOldRefreshToken);

        RefreshToken? addedRefreshToken = null;

        ConfigureSuccessfulRefresh(
            command,
            user,
            storedOldRefreshToken,
            rawNewRefreshToken,
            newRefreshTokenHash,
            accessToken,
            expiresAt);

        refreshTokenRepository
            .When(x => x.Add(Arg.Any<RefreshToken>()))
            .Do(callInfo => addedRefreshToken = callInfo.Arg<RefreshToken>());

        // Act
        await handler.Handle(command);

        // Assert
        refreshTokenRepository.Received(1)
            .Remove(storedOldRefreshToken);

        refreshTokenRepository.Received(1)
            .Add(Arg.Any<RefreshToken>());

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedRefreshToken);
        Assert.Equal(user.Guid, addedRefreshToken!.UserGuid);
        Assert.Equal(newRefreshTokenHash, addedRefreshToken.TokenHash);
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var rawOldRefreshToken = CreateToken('a');
        var oldRefreshTokenHash = CreateTokenHash('b');
        var rawNewRefreshToken = CreateToken('c');
        var newRefreshTokenHash = CreateTokenHash('d');
        var accessToken = CreateToken('e');
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var cancellationToken = new CancellationTokenSource().Token;
        var user = CreateUser();
        var storedOldRefreshToken = CreateStoredRefreshToken(
            user.Guid,
            oldRefreshTokenHash,
            DateTimeOffset.UtcNow.AddMinutes(1));
        var command = CreateCommand(rawOldRefreshToken);

        tokenHasher
            .Hash(rawOldRefreshToken)
            .Returns(oldRefreshTokenHash);

        refreshTokenRepository
            .GetByTokenHash(oldRefreshTokenHash, cancellationToken)
            .Returns(storedOldRefreshToken);

        userRepository
            .GetByGuid(user.Guid, cancellationToken)
            .Returns(user);

        refreshTokenGenerator
            .Generate()
            .Returns(rawNewRefreshToken);

        tokenHasher
            .Hash(rawNewRefreshToken)
            .Returns(newRefreshTokenHash);

        tokenGenerator
            .Generate(user)
            .Returns(new TokenGenerateResult(accessToken, expiresAt));

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await refreshTokenRepository.Received(1)
            .GetByTokenHash(oldRefreshTokenHash, cancellationToken);

        await userRepository.Received(1)
            .GetByGuid(user.Guid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_RemoveOldRefreshToken_SaveChanges_AndThrowUnauthorizedAccessFargoApplicationException_When_UserIsInactive()
    {
        // Arrange
        var rawOldRefreshToken = CreateToken('a');
        var oldRefreshTokenHash = CreateTokenHash('b');
        var user = CreateUser(isActive: false);
        var storedOldRefreshToken = CreateStoredRefreshToken(
                user.Guid,
                oldRefreshTokenHash,
                DateTimeOffset.UtcNow.AddMinutes(1));
        var command = CreateCommand(rawOldRefreshToken);

        ConfigureOldRefreshTokenHash(rawOldRefreshToken, oldRefreshTokenHash);

        refreshTokenRepository
            .GetByTokenHash(oldRefreshTokenHash, Arg.Any<CancellationToken>())
            .Returns(storedOldRefreshToken);

        userRepository
            .GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        refreshTokenRepository.Received(1)
            .Remove(storedOldRefreshToken);

        refreshTokenRepository.DidNotReceive()
            .Add(Arg.Any<RefreshToken>());

        refreshTokenGenerator.DidNotReceive()
            .Generate();

        tokenGenerator.DidNotReceive()
            .Generate(Arg.Any<User>());

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    private void ConfigureOldRefreshTokenHash(Token rawOldRefreshToken, TokenHash oldRefreshTokenHash)
    {
        tokenHasher
            .Hash(rawOldRefreshToken)
            .Returns(oldRefreshTokenHash);
    }

    private void ConfigureSuccessfulRefresh(
            RefreshCommand command,
            User user,
            RefreshToken storedOldRefreshToken,
            Token rawNewRefreshToken,
            TokenHash newRefreshTokenHash,
            Token accessToken,
            DateTimeOffset expiresAt)
    {
        ConfigureOldRefreshTokenHash(command.RefreshToken, storedOldRefreshToken.TokenHash);

        refreshTokenRepository
            .GetByTokenHash(storedOldRefreshToken.TokenHash, Arg.Any<CancellationToken>())
            .Returns(storedOldRefreshToken);

        userRepository
            .GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);

        refreshTokenGenerator
            .Generate()
            .Returns(rawNewRefreshToken);

        tokenHasher
            .Hash(rawNewRefreshToken)
            .Returns(newRefreshTokenHash);

        tokenGenerator
            .Generate(user)
            .Returns(new TokenGenerateResult(accessToken, expiresAt));
    }

    private static RefreshCommand CreateCommand(Token rawOldRefreshToken)
        => new(rawOldRefreshToken);

    private static User CreateUser(bool isActive = true)
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("user123"),
            PasswordHash = new PasswordHash(new string('p', PasswordHash.MinLength)),
            IsActive = isActive
        };
    }

    private static RefreshToken CreateStoredRefreshToken(
            Guid userGuid,
            TokenHash tokenHash,
            DateTimeOffset expiresAt)
    {
        return new RefreshToken
        {
            UserGuid = userGuid,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        };
    }

    private static Token CreateToken(char value)
        => new(new string(value, Token.MinLength));

    private static TokenHash CreateTokenHash(char value)
        => new(new string(value, TokenHash.MinLength));
}
