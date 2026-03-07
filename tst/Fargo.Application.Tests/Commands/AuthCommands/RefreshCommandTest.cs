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
    public sealed class RefreshCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_StoredRefreshTokenIsNotFound()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new RefreshCommandHandler(
                userRepository,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var rawRefreshToken = new Token(new string('a', Token.MinLength));
            var refreshTokenHash = new TokenHash(new string('b', TokenHash.MinLength));

            var command = new RefreshCommand(rawRefreshToken);

            tokenHasher
                .Hash(rawRefreshToken)
                .Returns(refreshTokenHash);

            refreshTokenRepository
                .GetByTokenHash(refreshTokenHash, Arg.Any<CancellationToken>())
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
            var userRepository = Substitute.For<IUserRepository>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new RefreshCommandHandler(
                userRepository,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var rawRefreshToken = new Token(new string('a', Token.MinLength));
            var oldRefreshTokenHash = new TokenHash(new string('b', TokenHash.MinLength));

            var storedOldRefreshToken = new RefreshToken
            {
                UserGuid = Guid.NewGuid(),
                TokenHash = oldRefreshTokenHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1)
            };

            var command = new RefreshCommand(rawRefreshToken);

            tokenHasher
                .Hash(rawRefreshToken)
                .Returns(oldRefreshTokenHash);

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
            var userRepository = Substitute.For<IUserRepository>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new RefreshCommandHandler(
                userRepository,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var rawRefreshToken = new Token(new string('a', Token.MinLength));
            var oldRefreshTokenHash = new TokenHash(new string('b', TokenHash.MinLength));

            var storedOldRefreshToken = new RefreshToken
            {
                UserGuid = Guid.NewGuid(),
                TokenHash = oldRefreshTokenHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1)
            };

            var command = new RefreshCommand(rawRefreshToken);

            tokenHasher
                .Hash(rawRefreshToken)
                .Returns(oldRefreshTokenHash);

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
            var userRepository = Substitute.For<IUserRepository>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new RefreshCommandHandler(
                userRepository,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var rawOldRefreshToken = new Token(new string('a', Token.MinLength));
            var oldRefreshTokenHash = new TokenHash(new string('b', TokenHash.MinLength));
            var rawNewRefreshToken = new Token(new string('c', Token.MinLength));
            var newRefreshTokenHash = new TokenHash(new string('d', TokenHash.MinLength));
            var accessToken = new Token(new string('e', Token.MinLength));
            var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

            var user = CreateUser();

            var storedOldRefreshToken = new RefreshToken
            {
                UserGuid = user.Guid,
                TokenHash = oldRefreshTokenHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1)
            };

            var command = new RefreshCommand(rawOldRefreshToken);

            tokenHasher
                .Hash(rawOldRefreshToken)
                .Returns(oldRefreshTokenHash);

            refreshTokenRepository
                .GetByTokenHash(oldRefreshTokenHash, Arg.Any<CancellationToken>())
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
            var userRepository = Substitute.For<IUserRepository>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new RefreshCommandHandler(
                userRepository,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var rawOldRefreshToken = new Token(new string('a', Token.MinLength));
            var oldRefreshTokenHash = new TokenHash(new string('b', TokenHash.MinLength));
            var rawNewRefreshToken = new Token(new string('c', Token.MinLength));
            var newRefreshTokenHash = new TokenHash(new string('d', TokenHash.MinLength));
            var accessToken = new Token(new string('e', Token.MinLength));
            var expiresAt = DateTimeOffset.UtcNow.AddHours(2);

            var user = CreateUser();

            var storedOldRefreshToken = new RefreshToken
            {
                UserGuid = user.Guid,
                TokenHash = oldRefreshTokenHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1)
            };

            RefreshToken? addedRefreshToken = null;

            var command = new RefreshCommand(rawOldRefreshToken);

            tokenHasher
                .Hash(rawOldRefreshToken)
                .Returns(oldRefreshTokenHash);

            refreshTokenRepository
                .GetByTokenHash(oldRefreshTokenHash, Arg.Any<CancellationToken>())
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
            var userRepository = Substitute.For<IUserRepository>();
            var tokenGenerator = Substitute.For<ITokenGenerator>();
            var refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
            var tokenHasher = Substitute.For<ITokenHasher>();
            var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();

            var handler = new RefreshCommandHandler(
                userRepository,
                tokenGenerator,
                refreshTokenGenerator,
                tokenHasher,
                refreshTokenRepository,
                unitOfWork);

            var rawOldRefreshToken = new Token(new string('a', Token.MinLength));
            var oldRefreshTokenHash = new TokenHash(new string('b', TokenHash.MinLength));
            var rawNewRefreshToken = new Token(new string('c', Token.MinLength));
            var newRefreshTokenHash = new TokenHash(new string('d', TokenHash.MinLength));
            var accessToken = new Token(new string('e', Token.MinLength));
            var expiresAt = DateTimeOffset.UtcNow.AddHours(2);
            var cancellationToken = new CancellationTokenSource().Token;

            var user = CreateUser();

            var storedOldRefreshToken = new RefreshToken
            {
                UserGuid = user.Guid,
                TokenHash = oldRefreshTokenHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1)
            };

            var command = new RefreshCommand(rawOldRefreshToken);

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