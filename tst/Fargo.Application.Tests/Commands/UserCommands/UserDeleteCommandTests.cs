using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.UserCommands;

public sealed class UserDeleteCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly UserDeleteCommandHandler handler;

    public UserDeleteCommandHandlerTests()
    {
        handler = new UserDeleteCommandHandler(
            userRepository,
            unitOfWork,
            currentUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository
            .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await userRepository.DidNotReceive()
            .GetByGuid(command.UserGuid, Arg.Any<CancellationToken>());

        userRepository.DidNotReceive()
            .Remove(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotFoundFargoApplicationException_When_TargetUserIsNotFound()
    {
        // Arrange
        var actor = CreateUser();
        actor.AddPermission(ActionType.DeleteUser);

        var targetUserGuid = Guid.NewGuid();
        var command = CreateCommand(targetUserGuid);

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        userRepository
            .GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotFoundFargoApplicationException>(act);

        userRepository.DidNotReceive()
            .Remove(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var actor = CreateUser();
        var targetUser = CreateUser();
        var command = CreateCommand(targetUser.Guid);

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        userRepository.DidNotReceive()
            .Remove(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserCannotDeleteSelfFargoDomainException_When_ActorTriesToDeleteHimself()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.DeleteUser);
        var command = CreateCommand(actor.Guid);

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);

        // importante: retorna outra instância com o mesmo Guid para validar por identidade
        var sameUserDifferentInstance = CreateUser(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor, sameUserDifferentInstance);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        var exception = await Assert.ThrowsAsync<UserCannotDeleteSelfFargoDomainException>(act);
        Assert.Equal(actor.Guid, exception.UserGuid);

        userRepository.DidNotReceive()
            .Remove(Arg.Any<User>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveUserAndSaveChanges_When_RequestIsValid()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.DeleteUser);
        var targetUser = CreateUser();
        var command = CreateCommand(targetUser.Guid);

        ConfigureCurrentUser(actor);
        ConfigureUserLookup(actor);
        ConfigureUserLookup(targetUser);

        // Act
        await handler.Handle(command);

        // Assert
        userRepository.Received(1)
            .Remove(targetUser);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.DeleteUser);
        var targetUser = CreateUser();
        var cancellationToken = new CancellationTokenSource().Token;
        var command = CreateCommand(targetUser.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        userRepository
            .GetByGuid(targetUser.Guid, cancellationToken)
            .Returns(targetUser);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, cancellationToken);

        await userRepository.Received(1)
            .GetByGuid(targetUser.Guid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    private void ConfigureCurrentUser(User actor)
    {
        currentUser.UserGuid.Returns(actor.Guid);
    }

    private void ConfigureUserLookup(User user)
    {
        userRepository
            .GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);
    }

    private static UserDeleteCommand CreateCommand(Guid? userGuid = null)
        => new(userGuid ?? Guid.NewGuid());

    private static User CreateUser(Guid? guid = null)
    {
        return new User
        {
            Guid = guid ?? Guid.NewGuid(),
            Nameid = new Nameid("user123"),
            PasswordHash = new PasswordHash(new string('a', PasswordHash.MinLength))
        };
    }

    private static User CreateUserWithPermission(ActionType action)
    {
        var user = CreateUser();
        user.AddPermission(action);
        return user;
    }
}