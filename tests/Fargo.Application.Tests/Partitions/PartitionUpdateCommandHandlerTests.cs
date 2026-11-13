using Fargo.Application.Commands.Partitions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.Partitions;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Partitions;

public class PartitionUpdateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly TestCurrentActor currentActor = new();

    private PartitionUpdateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            partitionRepository,
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
}
