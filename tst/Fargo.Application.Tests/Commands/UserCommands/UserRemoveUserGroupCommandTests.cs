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

public sealed class UserRemoveUserGroupCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly UserRemoveUserGroupCommandHandler handler;

    public UserRemoveUserGroupCommandHandlerTests()
    {
        handler = new UserRemoveUserGroupCommandHandler(
            userRepository,
            unitOfWork,
            currentUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var command = new UserRemoveUserGroupCommand(Guid.NewGuid(), Guid.NewGuid());

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(action);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserInactiveFargoDomainException_When_ActorIsInactive()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(actorGuid);
        actor.Deactivate();

        var command = new UserRemoveUserGroupCommand(Guid.NewGuid(), Guid.NewGuid());

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UserInactiveFargoDomainException>(action);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorHasNoPermission()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var actor = CreateUser(actorGuid);

        var command = new UserRemoveUserGroupCommand(Guid.NewGuid(), Guid.NewGuid());

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(action);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotFoundFargoApplicationException_When_TargetUserIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var targetUserGuid = Guid.NewGuid();

        var actor = CreateUser(actorGuid);
        actor.AddPermission(ActionType.ChangeUserGroupMembers);

        var command = new UserRemoveUserGroupCommand(targetUserGuid, Guid.NewGuid());

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userRepository.GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UserNotFoundFargoApplicationException>(action);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveUserGroupFromUser_When_CommandIsValid()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var targetUserGuid = Guid.NewGuid();
        var userGroupGuid = Guid.NewGuid();

        var actor = CreateUser(actorGuid);
        actor.AddPermission(ActionType.ChangeUserGroupMembers);

        var userGroup = CreateUserGroup(userGroupGuid);

        var user = CreateUser(targetUserGuid);
        user.AddGroup(userGroup);

        var command = new UserRemoveUserGroupCommand(targetUserGuid, userGroupGuid);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userRepository.GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(user.UserGroups, x => x.Guid == userGroupGuid);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_DoNothing_When_UserDoesNotBelongToGroup()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var targetUserGuid = Guid.NewGuid();
        var userGroupGuid = Guid.NewGuid();

        var actor = CreateUser(actorGuid);
        actor.AddPermission(ActionType.ChangeUserGroupMembers);

        var user = CreateUser(targetUserGuid);

        var command = new UserRemoveUserGroupCommand(targetUserGuid, userGroupGuid);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userRepository.GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Empty(user.UserGroups);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    private static User CreateUser(Guid guid)
    {
        return new User
        {
            Guid = guid,
            Nameid = new Nameid($"user-{guid:N}"),
            PasswordHash = new PasswordHash("hashedpasswordflsadjfasçdfjasdçlfjasçldfjasdlçfjasdçlfkjasdlçfaskljfasdlçfj")
        };
    }

    private static UserGroup CreateUserGroup(Guid guid)
    {
        return new UserGroup
        {
            Guid = guid,
            Nameid = new Nameid($"group-{guid:N}")
        };
    }
}