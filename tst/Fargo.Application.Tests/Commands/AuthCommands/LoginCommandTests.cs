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

public sealed class LoginCommandHandlerTests
{
    private static readonly Nameid validNameid = new("user123");
    private static readonly Password validPassword = new("Secure@123");

    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenGenerator tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly IRefreshTokenGenerator refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly ITokenHasher tokenHasher = Substitute.For<ITokenHasher>();
    private readonly IRefreshTokenRepository refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly LoginCommandHandler handler;

    public LoginCommandHandlerTests()
    {
        handler = new LoginCommandHandler(
            userRepository,
            passwordHasher,
            tokenGenerator,
            refreshTokenGenerator,
            tokenHasher,
            refreshTokenRepository,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_UserIsNotFound()
    {
        // Arrange
        var command = CreateCommand();

        userRepository
            .GetByNameid(command.Nameid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        passwordHasher.DidNotReceive()
            .Verify(Arg.Any<PasswordHash>(), Arg.Any<Password>());

        refreshTokenRepository.DidNotReceive()
            .Add(Arg.Any<RefreshToken>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_PasswordIsInvalid()
    {
        // Arrange
        var command = CreateCommand();
        var user = CreateUser();

        ConfigureUserFound(command, user);

        passwordHasher
            .Verify(user.PasswordHash, command.Password)
            .Returns(false);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        tokenGenerator.DidNotReceive()
            .Generate(Arg.Any<User>());

        refreshTokenGenerator.DidNotReceive()
            .Generate();

        tokenHasher.DidNotReceive()
            .Hash(Arg.Any<Token>());

        refreshTokenRepository.DidNotReceive()
            .Add(Arg.Any<RefreshToken>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnAuthResult_When_CredentialsAreValid()
    {
        // Arrange
        var command = CreateCommand();
        var user = CreateUser();
        var accessToken = CreateToken('a');
        var refreshToken = CreateToken('b');
        var refreshTokenHash = CreateTokenHash('c');
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

        ConfigureValidLogin(command, user, accessToken, refreshToken, refreshTokenHash, expiresAt);

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(expiresAt, result.ExpiresAt);
    }

    [Fact]
    public async Task Handle_Should_AddRefreshTokenAndSaveChanges_When_CredentialsAreValid()
    {
        // Arrange
        var command = CreateCommand();
        var user = CreateUser();
        var accessToken = CreateToken('a');
        var rawRefreshToken = CreateToken('b');
        var refreshTokenHash = CreateTokenHash('c');
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

        RefreshToken? addedRefreshToken = null;

        ConfigureValidLogin(command, user, accessToken, rawRefreshToken, refreshTokenHash, expiresAt);

        refreshTokenRepository
            .When(x => x.Add(Arg.Any<RefreshToken>()))
            .Do(callInfo => addedRefreshToken = callInfo.Arg<RefreshToken>());

        // Act
        await handler.Handle(command);

        // Assert
        refreshTokenRepository.Received(1)
            .Add(Arg.Any<RefreshToken>());

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedRefreshToken);
        Assert.Equal(user.Guid, addedRefreshToken!.UserGuid);
        Assert.Equal(refreshTokenHash, addedRefreshToken.TokenHash);
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var command = CreateCommand();
        var user = CreateUser();
        var accessToken = CreateToken('a');
        var rawRefreshToken = CreateToken('b');
        var refreshTokenHash = CreateTokenHash('c');
        var expiresAt = DateTimeOffset.UtcNow.AddHours(2);
        var cancellationToken = new CancellationTokenSource().Token;

        userRepository
            .GetByNameid(command.Nameid, cancellationToken)
            .Returns(user);

        passwordHasher
            .Verify(user.PasswordHash, command.Password)
            .Returns(true);

        tokenGenerator
            .Generate(user)
            .Returns(new TokenGenerateResult(accessToken, expiresAt));

        refreshTokenGenerator
            .Generate()
            .Returns(rawRefreshToken);

        tokenHasher
            .Hash(rawRefreshToken)
            .Returns(refreshTokenHash);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByNameid(command.Nameid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    private void ConfigureUserFound(LoginCommand command, User user)
    {
        userRepository
            .GetByNameid(command.Nameid, Arg.Any<CancellationToken>())
            .Returns(user);
    }

    private void ConfigureValidLogin(
        LoginCommand command,
        User user,
        Token accessToken,
        Token refreshToken,
        TokenHash refreshTokenHash,
        DateTimeOffset expiresAt)
    {
        ConfigureUserFound(command, user);

        passwordHasher
            .Verify(user.PasswordHash, command.Password)
            .Returns(true);

        tokenGenerator
            .Generate(user)
            .Returns(new TokenGenerateResult(accessToken, expiresAt));

        refreshTokenGenerator
            .Generate()
            .Returns(refreshToken);

        tokenHasher
            .Hash(refreshToken)
            .Returns(refreshTokenHash);
    }

    private static LoginCommand CreateCommand()
        => new(validNameid, validPassword);

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = validNameid,
            PasswordHash = new PasswordHash(new string('p', PasswordHash.MinLength))
        };
    }

    private static Token CreateToken(char value)
        => new(new string(value, Token.MinLength));

    private static TokenHash CreateTokenHash(char value)
        => new(new string(value, TokenHash.MinLength));
}