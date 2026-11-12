using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.Partitions;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Partitions;

public class PartitionSetParentCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly TestCurrentActor currentActor = new();

    private PartitionSetParentCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new PartitionService(partitionRepository),
            partitionRepository,
            currentActor);

    private static Partition ChildPartition(string name)
        => Partition.CreatePartition(new Name(name), Partition.CreateGlobalPartition(new Name("Global")));

    private Partition Existing(Partition partition)
    {
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));
        partitionRepository.GetDescendantGuidsAsync(partition.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        return partition;
    }

    private void ActorCanReach(params Partition[] partitions)
        => ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangePartitionParent],
            [FargoCoreWellKnowGuids.GlobalPartitionGuid, .. partitions.Select(p => p.Guid)]);

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldSetTheParentUnderTheHierarchyLock()
    {
        var partition = Existing(ChildPartition("child"));
        var newParent = Existing(ChildPartition("new-parent"));
        ActorCanReach(partition, newParent);

        await CreateHandler().HandleAsync(new PartitionSetParentCommand(partition.Guid, newParent.Guid));

        Assert.Equal(newParent.Guid, partition.ParentPartitionGuid);
        await partitionRepository.Received(1).AcquireHierarchyLockAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTheParentIsAlreadyTheCurrentOne_ShouldDoNothing()
    {
        var partition = Existing(ChildPartition("child"));
        ActorCanReach(partition);

        await CreateHandler().HandleAsync(new PartitionSetParentCommand(partition.Guid, partition.ParentPartitionGuid!.Value));

        await partitionRepository.DidNotReceive().AcquireHierarchyLockAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditPartition_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditPartition]);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionSetParentCommand(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenAssignmentCreatesACycle_ShouldThrow()
    {
        var partition = Existing(ChildPartition("child"));
        var newParent = Existing(ChildPartition("new-parent"));
        ActorCanReach(partition, newParent);
        // The new parent is a descendant of the partition being moved -> circular.
        partitionRepository.GetDescendantGuidsAsync(partition.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([newParent.Guid]));
        var originalParent = partition.ParentPartitionGuid;

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new PartitionSetParentCommand(partition.Guid, newParent.Guid)));

        Assert.Equal(originalParent, partition.ParentPartitionGuid);
    }

    [Fact]
    public async Task HandleAsync_WhenPartitionNotFound_ShouldThrow()
    {
        ActorCanReach();
        var missing = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionSetParentCommand(missing, Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenNewParentNotFound_ShouldThrow()
    {
        var partition = Existing(ChildPartition("child"));
        ActorCanReach(partition);
        var missing = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionSetParentCommand(partition.Guid, missing)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToThePartition_ShouldThrowAccessDenied()
    {
        var partition = Existing(ChildPartition("child"));
        ActorCanReach();

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionSetParentCommand(partition.Guid, Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToTheNewParent_ShouldThrowAccessDenied()
    {
        var partition = Existing(ChildPartition("child"));
        var newParent = Existing(ChildPartition("new-parent"));
        ActorCanReach(partition);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionSetParentCommand(partition.Guid, newParent.Guid)));
    }
}
