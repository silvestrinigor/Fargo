using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Domain.Tests.Services
{
    public sealed class UserServiceTests
    {
        [Fact]
        public async Task ValidateUserCreate_ShouldNotThrow_WhenNameidDoesNotExist()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var userService = new UserService(userRepository);

            var user = CreateUser("igor");

            userRepository
                .ExistsByNameid(user.Nameid, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            await userService.ValidateUserCreate(user);

            // Assert
            await userRepository.Received(1)
                .ExistsByNameid(user.Nameid, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ValidateUserCreate_ShouldThrowUserNameidAlreadyExistsDomainException_WhenNameidAlreadyExists()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var userService = new UserService(userRepository);

            var user = CreateUser("igor");

            userRepository
                .ExistsByNameid(user.Nameid, Arg.Any<CancellationToken>())
                .Returns(true);

            // Act
            Task act() => userService.ValidateUserCreate(user);

            // Assert
            await Assert.ThrowsAsync<UserNameidAlreadyExistsDomainException>(act);
        }

        [Fact]
        public async Task ValidateUserCreate_ShouldUseProvidedCancellationToken()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var userService = new UserService(userRepository);
            var cancellationToken = new CancellationTokenSource().Token;

            var user = CreateUser("igor");

            userRepository
                .ExistsByNameid(user.Nameid, cancellationToken)
                .Returns(false);

            // Act
            await userService.ValidateUserCreate(user, cancellationToken);

            // Assert
            await userRepository.Received(1)
                .ExistsByNameid(user.Nameid, cancellationToken);
        }

        [Fact]
        public void ValidateUserDelete_ShouldNotThrow_WhenActorDeletesAnotherUser()
        {
            // Arrange
            var user = CreateUser("target", Guid.NewGuid());
            var actor = CreateUser("actor", Guid.NewGuid());

            // Act
            UserService.ValidateUserDelete(user, actor);

            // Assert
        }

        [Fact]
        public void ValidateUserDelete_ShouldThrowUserCannotDeleteSelfFargoDomainException_WhenActorDeletesSelf()
        {
            // Arrange
            var sameGuid = Guid.NewGuid();
            var user = CreateUser("igor", sameGuid);
            var actor = CreateUser("igor-admin", sameGuid);

            // Act
            void act() => UserService.ValidateUserDelete(user, actor);

            // Assert
            var exception = Assert.Throws<UserCannotDeleteSelfFargoDomainException>(act);
            Assert.Equal(sameGuid, exception.UserGuid);
        }

        [Fact]
        public void ValidateUserDelete_ShouldThrow_WhenUsersHaveSameGuidEvenIfTheyAreDifferentInstances()
        {
            // Arrange
            var guid = Guid.NewGuid();

            var user = CreateUser("target", guid);
            var actor = CreateUser("actor", guid);

            // Act
            void act() => UserService.ValidateUserDelete(user, actor);

            // Assert
            var exception = Assert.Throws<UserCannotDeleteSelfFargoDomainException>(act);
            Assert.Equal(guid, exception.UserGuid);
        }

        [Fact]
        public void ValidateUserPermissionChange_ShouldNotThrow_WhenActorChangesAnotherUsersPermissions()
        {
            // Arrange
            var user = CreateUser("target", Guid.NewGuid());
            var actor = CreateUser("actor", Guid.NewGuid());

            // Act
            UserService.ValidateUserPermissionChange(user, actor);

            // Assert
        }

        [Fact]
        public void ValidateUserPermissionChange_ShouldThrowUserCannotChangeOwnPermissionsFargoDomainException_WhenActorChangesOwnPermissions()
        {
            // Arrange
            var sameGuid = Guid.NewGuid();
            var user = CreateUser("igor", sameGuid);
            var actor = CreateUser("igor-admin", sameGuid);

            // Act
            void act() => UserService.ValidateUserPermissionChange(user, actor);

            // Assert
            var exception = Assert.Throws<UserCannotChangeOwnPermissionsFargoDomainException>(act);
            Assert.Equal(sameGuid, exception.UserGuid);
        }

        [Fact]
        public void ValidateUserPermissionChange_ShouldThrow_WhenUsersHaveSameGuidEvenIfTheyAreDifferentInstances()
        {
            // Arrange
            var guid = Guid.NewGuid();

            var user = CreateUser("target", guid);
            var actor = CreateUser("actor", guid);

            // Act
            void act() => UserService.ValidateUserPermissionChange(user, actor);

            // Assert
            var exception = Assert.Throws<UserCannotChangeOwnPermissionsFargoDomainException>(act);
            Assert.Equal(guid, exception.UserGuid);
        }

        private static User CreateUser(string nameid, Guid? guid = null)
        {
            return new User
            {
                Guid = guid ?? Guid.NewGuid(),
                Nameid = new Nameid(nameid),
                PasswordHash = new PasswordHash(
                    "hashedushedvashedseshedbasesehdvareshedswasehd123vareshed")
            };
        }
    }
}