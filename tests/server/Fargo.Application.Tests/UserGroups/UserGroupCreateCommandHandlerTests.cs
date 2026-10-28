using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Application.UserGroups;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using NSubstitute;

namespace Fargo.Application.Tests.UserGroups;

public class UserGroupCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private UserGroupCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new UserGroupService(userGroupRepository),
            userGroupRepository,
            partitionRepository,
            auditLogRepository,
            currentActor);

    private static Partition GlobalPartition()
        => Partition.CreateGlobalPartition(new Name("Global"));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddUserGroupAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var command = new UserGroupCreateCommand(new UserGroupCreateDto(new Nameid("editors")));

        await CreateHandler().HandleAsync(command);

        userGroupRepository.Received(1).Add(Arg.Any<UserGroup>());
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(new UserGroupCreateDto(new Nameid("editors")))));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(new UserGroupCreateDto(new Nameid("editors")))));
    }

    [Fact]
    public async Task HandleAsync_WhenNameidAlreadyExists_ShouldThrowAndNotSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(new UserGroupCreateDto(new Nameid("editors")))));

        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenParentUserGroupNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var missingParent = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missingParent, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(
                    new UserGroupCreateDto(new Nameid("editors"), ParentUserGroup: missingParent))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToRequestedPartition_ShouldThrowAccessDenied()
    {
        // Actor has the permission and global access, but the requested partition
        // is a different one the actor cannot access.
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUserGroup]);
        userGroupRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var otherPartition = Partition.CreatePartition(new Name("Other"), GlobalPartition());
        partitionRepository.GetByGuidAsync(otherPartition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(otherPartition));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new UserGroupCreateCommand(
                    new UserGroupCreateDto(new Nameid("editors"), PartitionsToAdd: [otherPartition.Guid]))));

        userGroupRepository.DidNotReceive().Add(Arg.Any<UserGroup>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
