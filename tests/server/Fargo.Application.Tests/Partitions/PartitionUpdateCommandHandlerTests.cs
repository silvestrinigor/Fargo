using Fargo.Application.Common;
using Fargo.Application.Partitions;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.Users;
using NSubstitute;

namespace Fargo.Application.Tests.Partitions;

public class PartitionUpdateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private PartitionUpdateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new PartitionService(partitionRepository),
            partitionRepository,
            auditLogRepository,
            unitOfWork,
            currentActor);

    private static Partition GlobalPartition()
        => Partition.CreateGlobalPartition(new Name("Global"));

    private static Partition ChildPartition(string name = "Child")
        => Partition.CreatePartition(new Name(name), GlobalPartition());

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldUpdatePartitionAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditPartition]);
        var partition = ChildPartition();
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));
        // Grant access to the partition's own guid as well.
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditPartition],
            partitionAccesses: [FargoCoreWellKnowGuids.GlobalPartitionGuid, partition.Guid]);

        var command = new PartitionUpdateCommand(
            partition.Guid, new PartitionUpdateDto(new Name("Renamed"), new Description("Updated")));

        await CreateHandler().HandleAsync(command);

        Assert.Equal("Renamed", partition.Name.Value);
        Assert.Equal("Updated", partition.Description.Value);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionUpdateCommand(Guid.NewGuid(), new PartitionUpdateDto(new Name("X")))));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionUpdateCommand(Guid.NewGuid(), new PartitionUpdateDto(new Name("X")))));
    }

    [Fact]
    public async Task HandleAsync_WhenPartitionNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditPartition]);
        var missing = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionUpdateCommand(missing, new PartitionUpdateDto(new Name("X")))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditPartition], partitionAccesses: [Guid.NewGuid()]);
        var partition = ChildPartition();
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionUpdateCommand(partition.Guid, new PartitionUpdateDto(new Name("X")))));
    }

    [Fact]
    public async Task HandleAsync_WhenParentAssignmentCreatesCircularHierarchy_ShouldThrow()
    {
        var partition = ChildPartition();
        var newParent = ChildPartition("NewParent");
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditPartition],
            partitionAccesses:
            [
                FargoCoreWellKnowGuids.GlobalPartitionGuid, partition.Guid, newParent.Guid
            ]);
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));
        partitionRepository.GetByGuidAsync(newParent.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(newParent));
        // The new parent is a descendant of the partition being edited -> circular.
        partitionRepository.GetDescendantGuidsAsync(partition.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([newParent.Guid]));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(
                new PartitionUpdateCommand(
                    partition.Guid, new PartitionUpdateDto(ParentPartitionGuid: newParent.Guid))));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
