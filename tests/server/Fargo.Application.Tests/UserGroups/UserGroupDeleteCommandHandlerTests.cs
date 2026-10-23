using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Application.UserGroups;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.UserGroups;

public class UserGroupDeleteCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private UserGroupDeleteCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new UserGroupService(userGroupRepository),
            userGroupRepository,
            auditLogRepository,
            currentActor,
            unitOfWork,
            Substitute.For<ILogger<UserGroupDeleteCommandHandler>>());

    private static UserGroup NewUserGroup()
        => UserGroup.CreateUserGroup(new Nameid("editors"));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldRemoveUserGroupAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteUserGroup]);
        var userGroup = NewUserGroup();
        userGroupRepository.GetByGuidAsync(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(userGroup));
        userGroupRepository.HasChildrenUserGroupAsync(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        await CreateHandler().HandleAsync(new UserGroupDeleteCommand(userGroup.Guid));

        userGroupRepository.Received(1).Remove(userGroup);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenUserGroupNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteUserGroup]);
        var missing = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupDeleteCommand(missing)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.DeleteUserGroup], partitionAccesses: [Guid.NewGuid()]);
        var userGroup = NewUserGroup();
        userGroupRepository.GetByGuidAsync(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(userGroup));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupDeleteCommand(userGroup.Guid)));
    }

    [Fact]
    public async Task HandleAsync_WhenUserGroupHasChildren_ShouldThrowAndNotDelete()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteUserGroup]);
        var userGroup = NewUserGroup();
        userGroupRepository.GetByGuidAsync(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(userGroup));
        userGroupRepository.HasChildrenUserGroupAsync(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserGroupDeleteCommand(userGroup.Guid)));

        userGroupRepository.DidNotReceive().Remove(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
