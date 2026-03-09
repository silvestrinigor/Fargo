using Fargo.Domain.Entities;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace Fargo.Domain.Tests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task ValidateUserCreate_ShouldNotThrow_WhenNameidDoesNotExist()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var userService = new UserService(userRepository);

            var password = new PasswordHash("hashedushedvashedseshedbasesehdvareshedswasehd123vareshed");

            var nameid = new Nameid("igor");

            var user = new User
            {
                Nameid = nameid,
                PasswordHash = password
            };

            userRepository
                .ExistsByNameid(nameid, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            await userService.ValidateUserCreate(user);

            // Assert
            await userRepository.Received(1)
                .ExistsByNameid(nameid, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ValidateUserCreate_ShouldThrowException_WhenNameidAlreadyExists()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var userService = new UserService(userRepository);

            var password = new PasswordHash("hashedushedvashedseshedbasesehdvareshedswasehd123vareshed");

            var nameid = new Nameid("igor");

            var user = new User
            {
                Nameid = nameid,
                PasswordHash = password
            };

            userRepository
                .ExistsByNameid(nameid, Arg.Any<CancellationToken>())
                .Returns(true);

            // Act + Assert
            await Assert.ThrowsAsync<UserNameidAlreadyExistsDomainException>(
                () => userService.ValidateUserCreate(user)
            );
        }

        [Fact]
        public async Task ValidateUserCreate_ShouldCheckRepositoryWithCorrectNameid()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var userService = new UserService(userRepository);

            var password = new PasswordHash("hashedushedvashedseshedbasesehdvareshedswasehd123vareshed");

            var nameid = new Nameid("igor");

            var user = new User
            {
                Nameid = nameid,
                PasswordHash = password
            };

            userRepository
                .ExistsByNameid(nameid, Arg.Any<CancellationToken>())
                .Returns(false);

            // Act
            await userService.ValidateUserCreate(user);

            // Assert
            await userRepository.Received(1)
                .ExistsByNameid(nameid, Arg.Any<CancellationToken>());
        }
    }
}