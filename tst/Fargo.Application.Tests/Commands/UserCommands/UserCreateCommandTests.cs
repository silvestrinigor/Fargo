using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.UserCommands
{
    public sealed class UserCreateCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessFargoApplicationException_WhenActorIsNotFound()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var currentUser = Substitute.For<ICurrentUser>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var userService = new UserService(userRepository);

            var actorGuid = Guid.NewGuid();

            currentUser.UserGuid.Returns(actorGuid);

            userRepository
                .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
                .Returns((User?)null);

            var handler = new UserCreateCommandHandler(
                    userService,
                    userRepository,
                    unitOfWork,
                    currentUser,
                    passwordHasher
                    );

            var command = new UserCreateCommand(
                    new UserCreateModel(
                        new Nameid("igor"),
                        new Password("fasdlfkasj@123456"),
                        Permissions: []
                        )
                    );

            // Act + Assert
            await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(
                    () => handler.Handle(command)
                    );

            await unitOfWork.DidNotReceive()
                .SaveChanges(Arg.Any<CancellationToken>());

            passwordHasher.DidNotReceive()
                .Hash(Arg.Any<Password>());

            userRepository.DidNotReceive()
                .Add(Arg.Any<User>());
        }

        [Fact]
        public async Task Handle_ShouldHashPassword_AddUser_SaveChanges_AndReturnUserGuid_WhenCommandIsValid()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var currentUser = Substitute.For<ICurrentUser>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var userService = new UserService(userRepository);

            var actorGuid = Guid.NewGuid();
            var hashedPassword = "hashed-passwordfisdafasdlfjasdlfjweoifjasldkfjaslfjasdlfkjas";

            currentUser.UserGuid.Returns(actorGuid);

            var actor = new User
            {
                Nameid = new Nameid("admin"),
                PasswordHash = new PasswordHash("actor-password-hashfasfdasdfasdfasdfafdsaflkadsfjas")
            };

            actor.AddPermission(ActionType.CreateUser);

            userRepository
                .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
                .Returns(actor);

            passwordHasher
                .Hash(Arg.Any<Password>())
                .Returns(new PasswordHash(hashedPassword));

            User? addedUser = null;

            userRepository
                .When(x => x.Add(Arg.Any<User>()))
                .Do(callInfo => addedUser = callInfo.Arg<User>());

            var handler = new UserCreateCommandHandler(
                    userService,
                    userRepository,
                    unitOfWork,
                    currentUser,
                    passwordHasher
                    );

            var nameid = new Nameid("igor");
            var password = new Password("asjfasl#123456");

            var command = new UserCreateCommand(
                    new UserCreateModel(
                        nameid,
                        password,
                        Permissions: []
                        )
                    );

            // Act
            var result = await handler.Handle(command);

            // Assert
            await userRepository.Received(1)
                .GetByGuid(actorGuid, Arg.Any<CancellationToken>());

            passwordHasher.Received(1)
                .Hash(password);

            userRepository.Received(1)
                .Add(Arg.Any<User>());

            await unitOfWork.Received(1)
                .SaveChanges(Arg.Any<CancellationToken>());

            Assert.NotNull(addedUser);
            Assert.Equal(nameid, addedUser.Nameid);
            Assert.Equal(hashedPassword, addedUser.PasswordHash);
            Assert.Equal(addedUser.Guid, result);
        }

        [Fact]
        public async Task Handle_ShouldAddAllPermissionsFromCommandToCreatedUser()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var currentUser = Substitute.For<ICurrentUser>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var userService = new UserService(userRepository);

            var actorGuid = Guid.NewGuid();

            currentUser.UserGuid.Returns(actorGuid);

            var actor = new User
            {
                Nameid = new Nameid("admin"),
                PasswordHash = new PasswordHash("actor-password-hashfasfdasdfasdfasdfafdsaflkadsfjas")
            };

            actor.AddPermission(ActionType.CreateUser);

            userRepository
                .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
                .Returns(actor);

            passwordHasher
                .Hash(Arg.Any<Password>())
                .Returns(new PasswordHash("hashed-passwordfasdfasdlfjasdlfkasjdlfjdsalkfjasdlfjasdkj"));

            User? addedUser = null;

            userRepository
                .When(x => x.Add(Arg.Any<User>()))
                .Do(callInfo => addedUser = callInfo.Arg<User>());

            var handler = new UserCreateCommandHandler(
                    userService,
                    userRepository,
                    unitOfWork,
                    currentUser,
                    passwordHasher
                    );

            var permissions = new[]
            {
                ActionType.CreateArticle,
                ActionType.CreateArticle,
                ActionType.DeleteItem
            };

            var command = new UserCreateCommand(
                    new UserCreateModel(
                        new Nameid("igor"),
                        new Password("askfsakldlj@123456"),
                        Permissions: permissions
                        )
                    );

            // Act
            await handler.Handle(command);

            // Assert
            Assert.NotNull(addedUser);

            Assert.Contains(
                    addedUser!.UserPermissions,
                    x => x.Action == ActionType.CreateArticle
                    );

            Assert.Contains(
                    addedUser.UserPermissions,
                    x => x.Action == ActionType.CreateArticle
                    );

            Assert.Contains(
                    addedUser.UserPermissions,
                    x => x.Action == ActionType.DeleteItem
                    );
        }

        [Fact]
        public async Task Handle_ShouldCreateUserWithoutPermissions_WhenCommandPermissionsIsNull()
        {
            // Arrange
            var userRepository = Substitute.For<IUserRepository>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var currentUser = Substitute.For<ICurrentUser>();
            var passwordHasher = Substitute.For<IPasswordHasher>();
            var userService = new UserService(userRepository);

            var actorGuid = Guid.NewGuid();

            currentUser.UserGuid.Returns(actorGuid);

            var actor = new User
            {
                Nameid = new Nameid("admin"),
                PasswordHash = new PasswordHash("actor-password-hashfasfdasdfasdfasdfafdsaflkadsfjas")
            };

            actor.AddPermission(ActionType.CreateUser);

            userRepository
                .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
                .Returns(actor);

            passwordHasher
                .Hash(Arg.Any<Password>())
                .Returns(new PasswordHash("haspaswordasdfasdfasdfasdflsfhasdflasjdflafafasdfaslkfjlsadkls"));

            User? addedUser = null;

            userRepository
                .When(x => x.Add(Arg.Any<User>()))
                .Do(callInfo => addedUser = callInfo.Arg<User>());

            var handler = new UserCreateCommandHandler(
                    userService,
                    userRepository,
                    unitOfWork,
                    currentUser,
                    passwordHasher
                    );

            var command = new UserCreateCommand(
                    new UserCreateModel(
                        new Nameid("igor"),
                        new Password("falfjasl#123456"),
                        null
                        )
                    );

            // Act
            await handler.Handle(command);

            // Assert
            Assert.NotNull(addedUser);
            Assert.Empty(addedUser.UserPermissions);
        }
    }
}