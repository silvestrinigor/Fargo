using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.UserGroups;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.UserGroups;

public class UserGroupSetActiveCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly TestCurrentActor currentActor = new();

    private UserGroupSetActiveCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            userGroupRepository,
            currentActor);

    private static UserGroup NewUserGroup()
        => UserGroup.CreateUserGroup(new Nameid("editors"));

    private void Existing(UserGroup userGroup)
        => userGroupRepository.GetByGuidAsync(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(userGroup));

    [Fact]
    public async Task HandleAsync_WhenDeactivating_ShouldDeactivateUserGroup()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateUserGroup]);
        var userGroup = NewUserGroup();
        Existing(userGroup);

        await CreateHandler().HandleAsync(new UserGroupSetActiveCommand(userGroup.Guid, false));

        Assert.False(userGroup.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WhenActivating_ShouldActivateUserGroup()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateUserGroup]);
        var userGroup = NewUserGroup();
        userGroup.Deactivate();
        Existing(userGroup);

        await CreateHandler().HandleAsync(new UserGroupSetActiveCommand(userGroup.Guid, true));

        Assert.True(userGroup.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditUserGroup_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUserGroup]);
        var userGroup = NewUserGroup();
        Existing(userGroup);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetActiveCommand(userGroup.Guid, false)));

        Assert.True(userGroup.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WhenUserGroupNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateUserGroup]);
        var missing = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetActiveCommand(missing, false)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ActivateUserGroup], partitionAccesses: [Guid.NewGuid()]);
        var userGroup = NewUserGroup();
        Existing(userGroup);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetActiveCommand(userGroup.Guid, false)));
    }

    [Fact]
    public async Task HandleAsync_WhenDeactivatingAdministratorsGroup_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateUserGroup]);
        var administrators = UserGroup.CreateAdministratorsUserGroup(new Nameid("administrators"));
        Existing(administrators);

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserGroupSetActiveCommand(administrators.Guid, false)));

        Assert.True(administrators.IsActive);
    }
}
