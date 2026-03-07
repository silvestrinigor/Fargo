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

namespace Fargo.Application.Tests.Commands.AuthCommands
{
    public sealed class LoginCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_UserIsNotFound()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new LoginCommandHandler(
                userRepository,
                passwordHasher,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var command = new LoginCommand(
                new Nameid("igor123"),
                new Password("Secure@123"));

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
            var userRepository = Substitute.For<IUserRepository>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new LoginCommandHandler(
                userRepository,
                passwordHasher,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var command = new LoginCommand(
                new Nameid("igor123"),
                new Password("Secure@123"));

            var user = CreateUser();

            userRepository
                .GetByNameid(command.Nameid, Arg.Any<CancellationToken>())
                .Returns(user);

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
            var userRepository = Substitute.For<IUserRepository>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new LoginCommandHandler(
                userRepository,
                passwordHasher,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var command = new LoginCommand(
                new Nameid("igor123"),
                new Password("Secure@123"));

            var user = CreateUser();
            var accessToken = new Token(new string('a', Token.MinLength));
            var refreshToken = new Token(new string('b', Token.MinLength));
            var refreshTokenHash = new TokenHash(new string('c', TokenHash.MinLength));
            var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

            userRepository
                .GetByNameid(command.Nameid, Arg.Any<CancellationToken>())
                .Returns(user);

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
            var userRepository = Substitute.For<IUserRepository>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new LoginCommandHandler(
                userRepository,
                passwordHasher,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var command = new LoginCommand(
                new Nameid("igor123"),
                new Password("Secure@123"));

            var user = CreateUser();
            var accessToken = new Token(new string('a', Token.MinLength));
            var rawRefreshToken = new Token(new string('b', Token.MinLength));
            var refreshTokenHash = new TokenHash(new string('c', TokenHash.MinLength));
            var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

            RefreshToken? addedRefreshToken = null;

            userRepository
                .GetByNameid(command.Nameid, Arg.Any<CancellationToken>())
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
            var userRepository = Substitute.For<IUserRepository>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new LoginCommandHandler(
                userRepository,
                passwordHasher,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var command = new LoginCommand(
                new Nameid("igor123"),
                new Password("Secure@123"));

            var user = CreateUser();
            var accessToken = new Token(new string('a', Token.MinLength));
            var rawRefreshToken = new Token(new string('b', Token.MinLength));
            var refreshTokenHash = new TokenHash(new string('c', TokenHash.MinLength));
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

        private static User CreateUser()
        {
            return new User
            {
                Guid = Guid.NewGuid(),
                Nameid = new Nameid("igor123"),
                PasswordHash = new PasswordHash(new string('p', PasswordHash.MinLength))
            };
        }
    }
}