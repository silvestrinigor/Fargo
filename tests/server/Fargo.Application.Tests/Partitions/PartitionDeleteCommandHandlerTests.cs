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

public class PartitionDeleteCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private PartitionDeleteCommandHandler CreateHandler()
        => new(
            new PartitionService(partitionRepository),
            ActorTestFactory.Resolver(userRepository),
            partitionRepository,
            auditLogRepository,
            currentActor);

    private static Partition GlobalPartition()
        => Partition.CreateGlobalPartition(new Name("Global"));

    private static Partition ChildPartition(Partition parent, string name = "Child")
        => Partition.CreatePartition(new Name(name), parent);

    private void GrantAccess(Partition target)
        => ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.DeletePartition],
            partitionAccesses: [FargoCoreWellKnowGuids.GlobalPartitionGuid, target.Guid]);

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldRemovePartitionAndAuditAndSave()
    {
        var global = GlobalPartition();
        var partition = ChildPartition(global);
        GrantAccess(partition);
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));
        partitionRepository.GetByGuidAsync(FargoCoreWellKnowGuids.GlobalPartitionGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(global));
        partitionRepository.HasChildrenAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        await CreateHandler().HandleAsync(new PartitionDeleteCommand(partition.Guid));

        partitionRepository.Received(1).Remove(partition);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenPartitionNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeletePartition]);
        var missing = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionDeleteCommand(missing)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.DeletePartition], partitionAccesses: [Guid.NewGuid()]);
        var global = GlobalPartition();
        var partition = ChildPartition(global);
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new PartitionDeleteCommand(partition.Guid)));
    }

    [Fact]
    public async Task HandleAsync_WhenPartitionHasNoParent_ShouldThrow()
    {
        // The global partition has no parent -> cannot be deleted.
        var global = GlobalPartition();
        GrantAccess(global);
        partitionRepository.GetByGuidAsync(FargoCoreWellKnowGuids.GlobalPartitionGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(global));

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionDeleteCommand(FargoCoreWellKnowGuids.GlobalPartitionGuid)));

        partitionRepository.DidNotReceive().Remove(Arg.Any<Partition>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPartitionHasChildren_ShouldThrowAndNotDelete()
    {
        var global = GlobalPartition();
        var partition = ChildPartition(global);
        GrantAccess(partition);
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));
        partitionRepository.GetByGuidAsync(FargoCoreWellKnowGuids.GlobalPartitionGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(global));
        partitionRepository.HasChildrenAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new PartitionDeleteCommand(partition.Guid)));

        partitionRepository.DidNotReceive().Remove(Arg.Any<Partition>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
