using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.AuthCommands;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.AuthCommands;

public sealed class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_DoNothing_When_RefreshTokenDoesNotExist()
    {
        // Arrange
        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        var tokenHasher = Substitute.For<ITokenHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new LogoutCommandHandler(
            refreshTokenRepository,
            tokenHasher,
            unitOfWork);

        var rawToken = new Token(new string('a', Token.MinLength));
        var tokenHash = new TokenHash(new string('b', TokenHash.MinLength));

        var command = new LogoutCommand(rawToken);

        tokenHasher
            .Hash(rawToken)
            .Returns(tokenHash);

        refreshTokenRepository
            .GetByTokenHash(tokenHash, Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        // Act
        await handler.Handle(command);

        // Assert
        refreshTokenRepository.DidNotReceive()
            .Remove(Arg.Any<RefreshToken>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveRefreshToken_When_TokenExists()
    {
        // Arrange
        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        var tokenHasher = Substitute.For<ITokenHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new LogoutCommandHandler(
            refreshTokenRepository,
            tokenHasher,
            unitOfWork);

        var rawToken = new Token(new string('a', Token.MinLength));
        var tokenHash = new TokenHash(new string('b', TokenHash.MinLength));

        var storedToken = new RefreshToken
        {
            UserGuid = Guid.NewGuid(),
            TokenHash = tokenHash
        };

        var command = new LogoutCommand(rawToken);

        tokenHasher
            .Hash(rawToken)
            .Returns(tokenHash);

        refreshTokenRepository
            .GetByTokenHash(tokenHash, Arg.Any<CancellationToken>())
            .Returns(storedToken);

        // Act
        await handler.Handle(command);

        // Assert
        refreshTokenRepository.Received(1)
            .Remove(storedToken);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_HashRefreshToken()
    {
        // Arrange
        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        var tokenHasher = Substitute.For<ITokenHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new LogoutCommandHandler(
            refreshTokenRepository,
            tokenHasher,
            unitOfWork);

        var rawToken = new Token(new string('a', Token.MinLength));
        var tokenHash = new TokenHash(new string('b', TokenHash.MinLength));

        var command = new LogoutCommand(rawToken);

        tokenHasher
            .Hash(rawToken)
            .Returns(tokenHash);

        refreshTokenRepository
            .GetByTokenHash(tokenHash, Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        // Act
        await handler.Handle(command);

        // Assert
        tokenHasher.Received(1)
            .Hash(rawToken);
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        var tokenHasher = Substitute.For<ITokenHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new LogoutCommandHandler(
            refreshTokenRepository,
            tokenHasher,
            unitOfWork);

        var rawToken = new Token(new string('a', Token.MinLength));
        var tokenHash = new TokenHash(new string('b', TokenHash.MinLength));
        var cancellationToken = new CancellationTokenSource().Token;

        var command = new LogoutCommand(rawToken);

        tokenHasher
            .Hash(rawToken)
            .Returns(tokenHash);

        refreshTokenRepository
            .GetByTokenHash(tokenHash, cancellationToken)
            .Returns((RefreshToken?)null);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await refreshTokenRepository.Received(1)
            .GetByTokenHash(tokenHash, cancellationToken);
    }
}