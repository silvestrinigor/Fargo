using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.UserGroupCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.UserGroupCommands;

public sealed class UserGroupDeleteCommandHandlerTests
{
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly UserGroupDeleteCommandHandler handler;

    public UserGroupDeleteCommandHandlerTests()
    {
        handler = new UserGroupDeleteCommandHandler(
            userGroupRepository,
            userRepository,
            unitOfWork,
            currentUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var command = new UserGroupDeleteCommand(Guid.NewGuid());

        currentUser.UserGuid.Returns(actorGuid);

        userRepository.GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(action);

        userGroupRepository.DidNotReceive().Remove(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserInactiveFargoDomainException_When_ActorIsInactive()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.DeleteUserGroup);
        actor.IsActive = false;

        var command = new UserGroupDeleteCommand(Guid.NewGuid());

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UserInactiveFargoDomainException>(action);

        userGroupRepository.DidNotReceive().Remove(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var actor = CreateActor();
        var command = new UserGroupDeleteCommand(Guid.NewGuid());

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(action);

        userGroupRepository.DidNotReceive().Remove(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserGroupNotFoundFargoApplicationException_When_UserGroupIsNotFound()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.DeleteUserGroup);
        var userGroupGuid = Guid.NewGuid();
        var command = new UserGroupDeleteCommand(userGroupGuid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(userGroupGuid, Arg.Any<CancellationToken>())
            .Returns((UserGroup?)null);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<UserGroupNotFoundFargoApplicationException>(action);
        Assert.Equal(userGroupGuid, exception.UserGroupGuid);

        userGroupRepository.DidNotReceive().Remove(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserCannotDeleteParentUserGroupFargoDomainException_When_ActorBelongsToUserGroup()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.DeleteUserGroup);
        var userGroup = CreateUserGroup();

        actor.AddGroup(userGroup);

        var command = new UserGroupDeleteCommand(userGroup.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await Assert.ThrowsAsync<UserCannotDeleteParentUserGroupFargoDomainException>(action);
        Assert.Equal(userGroup.Guid, exception.UserGroupGuid);

        userGroupRepository.DidNotReceive().Remove(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveUserGroupAndSaveChanges_When_CommandIsValid()
    {
        // Arrange
        var actor = CreateActorWithPermissions(ActionType.DeleteUserGroup);
        var userGroup = CreateUserGroup();

        var command = new UserGroupDeleteCommand(userGroup.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository.GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        userGroupRepository.Received(1).Remove(userGroup);
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    private static User CreateActor()
    {
        return new User
        {
            Nameid = Nameid.FromString("actor"),
            PasswordHash = new("hashedpasswordfdaslfjaskfjasdlfjasdlfaksjdflasdjflaskfjalsdfjasldfjasldfjasdlfkjasdlf"),
            DefaultPasswordExpirationPeriod = TimeSpan.FromDays(30),
            Description = Description.Empty,
            IsActive = true
        };
    }

    private static User CreateActorWithPermissions(params ActionType[] actions)
    {
        var actor = CreateActor();

        foreach (var action in actions.Distinct())
        {
            actor.AddPermission(action);
        }

        return actor;
    }

    private static UserGroup CreateUserGroup()
    {
        return new UserGroup
        {
            Nameid = Nameid.FromString("group.test"),
            Description = Description.FromString("Test group"),
            IsActive = true
        };
    }
}