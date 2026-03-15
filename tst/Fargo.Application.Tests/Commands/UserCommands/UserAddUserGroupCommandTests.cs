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

public sealed class UserAddUserGroupCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly UserAddUserGroupCommandHandler handler;

    public UserAddUserGroupCommandHandlerTests()
    {
        handler = new UserAddUserGroupCommandHandler(
            userRepository,
            userGroupRepository,
            unitOfWork,
            currentUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var command = new UserAddUserGroupCommand(Guid.NewGuid(), Guid.NewGuid());

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

        var command = new UserAddUserGroupCommand(Guid.NewGuid(), Guid.NewGuid());

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

        var command = new UserAddUserGroupCommand(Guid.NewGuid(), Guid.NewGuid());

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
        var userGroupGuid = Guid.NewGuid();

        var actor = CreateUser(actorGuid);
        actor.AddPermission(ActionType.ChangeUserGroupMembers);

        var command = new UserAddUserGroupCommand(targetUserGuid, userGroupGuid);

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
    public async Task Handle_Should_ThrowUserGroupNotFoundFargoApplicationException_When_UserGroupIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var targetUserGuid = Guid.NewGuid();
        var userGroupGuid = Guid.NewGuid();

        var actor = CreateUser(actorGuid);
        actor.AddPermission(ActionType.ChangeUserGroupMembers);

        var user = CreateUser(targetUserGuid);

        var command = new UserAddUserGroupCommand(targetUserGuid, userGroupGuid);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userRepository.GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns(user);

        userGroupRepository.GetByGuid(userGroupGuid, Arg.Any<CancellationToken>())
            .Returns((UserGroup?)null);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UserGroupNotFoundFargoApplicationException>(action);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AddUserGroupToUser_When_CommandIsValid()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var targetUserGuid = Guid.NewGuid();
        var userGroupGuid = Guid.NewGuid();

        var actor = CreateUser(actorGuid);
        actor.AddPermission(ActionType.ChangeUserGroupMembers);

        var user = CreateUser(targetUserGuid);
        var userGroup = CreateUserGroup(userGroupGuid);

        var command = new UserAddUserGroupCommand(targetUserGuid, userGroupGuid);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userRepository.GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns(user);

        userGroupRepository.GetByGuid(userGroupGuid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains(user.UserGroups, x => x.Guid == userGroupGuid);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotDuplicateUserGroup_When_UserAlreadyBelongsToGroup()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var targetUserGuid = Guid.NewGuid();
        var userGroupGuid = Guid.NewGuid();

        var actor = CreateUser(actorGuid);
        actor.AddPermission(ActionType.ChangeUserGroupMembers);

        var userGroup = CreateUserGroup(userGroupGuid);

        var user = CreateUser(targetUserGuid);
        user.UserGroups.Add(userGroup);

        var command = new UserAddUserGroupCommand(targetUserGuid, userGroupGuid);

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userRepository.GetByGuid(targetUserGuid, Arg.Any<CancellationToken>())
            .Returns(user);

        userGroupRepository.GetByGuid(userGroupGuid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(user.UserGroups, x => x.Guid == userGroupGuid);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    private static User CreateUser(Guid guid)
    {
        return new User
        {
            Guid = guid,
            Nameid = new Nameid($"user-{guid:N}"),
            PasswordHash = new PasswordHash("hashedpasswordfadsflasdfjasdçfjasfçjasdçlfjasdflasdjfalskfjlsdlfdasjl")
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