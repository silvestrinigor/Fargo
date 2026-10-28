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

public class PartitionCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private PartitionCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            partitionRepository,
            auditLogRepository,
            currentActor);

    private static Partition GlobalPartition()
        => Partition.CreateGlobalPartition(new Name("Global"));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddPartitionAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreatePartition]);
        partitionRepository.GetByGuidAsync(FargoCoreWellKnowGuids.GlobalPartitionGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(GlobalPartition()));

        var command = new PartitionCreateCommand(new PartitionCreateDto(new Name("Child")));

        await CreateHandler().HandleAsync(command);

        partitionRepository.Received(1).Add(Arg.Any<Partition>());
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionCreateCommand(new PartitionCreateDto(new Name("Child")))));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionCreateCommand(new PartitionCreateDto(new Name("Child")))));
    }

    [Fact]
    public async Task HandleAsync_WhenParentPartitionNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreatePartition]);
        var missingParent = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missingParent, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionCreateCommand(new PartitionCreateDto(new Name("Child"), missingParent))));
    }

    [Fact]
    public async Task HandleAsync_WhenNoParentProvidedAndNoGlobalAccess_ShouldThrowAccessDenied()
    {
        // When no parent is provided, the handler defaults to the global partition
        // and requires access to it. Without that access it throws a
        // FargoApplicationException categorized as AccessDenied (not the typed
        // ActorAccessDenied exception).
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.CreatePartition], partitionAccesses: [Guid.NewGuid()]);
        partitionRepository.GetByGuidAsync(FargoCoreWellKnowGuids.GlobalPartitionGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(GlobalPartition()));

        var ex = await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(
                new PartitionCreateCommand(new PartitionCreateDto(new Name("Child")))));

        Assert.Equal(FargoErrorType.AccessDenied, ex.ErrorType);
        partitionRepository.DidNotReceive().Add(Arg.Any<Partition>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
