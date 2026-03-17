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
    private readonly IRefreshTokenRepository refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly ITokenHasher tokenHasher = Substitute.For<ITokenHasher>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly LogoutCommandHandler handler;

    public LogoutCommandHandlerTests()
    {
        handler = new LogoutCommandHandler(
            refreshTokenRepository,
            tokenHasher,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_DoNothing_When_RefreshTokenDoesNotExist()
    {
        // Arrange
        var rawToken = CreateToken('a');
        var tokenHash = CreateTokenHash('b');
        var command = CreateCommand(rawToken);

        ConfigureTokenHash(rawToken, tokenHash);

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
        var rawToken = CreateToken('a');
        var tokenHash = CreateTokenHash('b');
        var storedToken = CreateStoredRefreshToken(tokenHash);
        var command = CreateCommand(rawToken);

        ConfigureTokenHash(rawToken, tokenHash);

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
        var rawToken = CreateToken('a');
        var tokenHash = CreateTokenHash('b');
        var command = CreateCommand(rawToken);

        ConfigureTokenHash(rawToken, tokenHash);

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
        var rawToken = CreateToken('a');
        var tokenHash = CreateTokenHash('b');
        var cancellationToken = new CancellationTokenSource().Token;
        var command = CreateCommand(rawToken);

        ConfigureTokenHash(rawToken, tokenHash);

        refreshTokenRepository
            .GetByTokenHash(tokenHash, cancellationToken)
            .Returns((RefreshToken?)null);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await refreshTokenRepository.Received(1)
            .GetByTokenHash(tokenHash, cancellationToken);
    }

    private void ConfigureTokenHash(Token rawToken, TokenHash tokenHash)
    {
        tokenHasher
            .Hash(rawToken)
            .Returns(tokenHash);
    }

    private static LogoutCommand CreateCommand(Token rawToken)
        => new(rawToken);

    private static RefreshToken CreateStoredRefreshToken(TokenHash tokenHash)
    {
        return new RefreshToken
        {
            UserGuid = Guid.NewGuid(),
            TokenHash = tokenHash
        };
    }

    private static Token CreateToken(char value)
        => new(new string(value, Token.MinLength));

    private static TokenHash CreateTokenHash(char value)
        => new(new string(value, TokenHash.MinLength));
}
