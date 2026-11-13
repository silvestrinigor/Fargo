using Fargo.Application.Commands.Items;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemSetPartitionsCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ItemSetPartitionsCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            partitionRepository,
            currentActor);

    private static Item NewTarget()
        => Item.CreateItem(Article.NewArticle(new Name("Widget")));

    private static Partition NewPartition(string name)
        => Partition.CreatePartition(new Name(name), Partition.CreateGlobalPartition(new Name("global")));

    private Partition Existing(Partition partition)
    {
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        return partition;
    }

    private void Existing(Item target)
        => itemRepository.GetByGuidAsync(target.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(target));

    private void ActorCanReach(params Partition[] partitions)
        => ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeItemPartitions],
            [FargoCoreWellKnowGuids.GlobalPartitionGuid, .. partitions.Select(p => p.Guid)]);

    [Fact]
    public async Task HandleAsync_WhenAddingAndRemoving_ShouldApplyBoth()
    {
        var toAdd = Existing(NewPartition("to-add"));
        var toRemove = Existing(NewPartition("to-remove"));
        ActorCanReach(toAdd, toRemove);
        var target = NewTarget();
        target.AddPartition(toRemove);
        Existing(target);

        await CreateHandler().HandleAsync(new ItemSetPartitionsCommand(
            target.Guid, new PartitionMembershipsChangeDto(PartitionsToAdd: [toAdd.Guid], PartitionsToRemove: [toRemove.Guid])));

        Assert.Contains(toAdd.Guid, target.PartitionGuids);
        Assert.DoesNotContain(toRemove.Guid, target.PartitionGuids);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasTheEditPermission_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditItem]);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetPartitionsCommand(
                Guid.NewGuid(), new PartitionMembershipsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToThePartition_ShouldThrowAccessDenied()
    {
        var restricted = Existing(NewPartition("restricted"));
        ActorCanReach();
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetPartitionsCommand(
                target.Guid, new PartitionMembershipsChangeDto(PartitionsToAdd: [restricted.Guid]))));

        Assert.DoesNotContain(restricted.Guid, target.PartitionGuids);
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingAPartitionTheActorCannotReach_ShouldThrowAccessDenied()
    {
        var restricted = Existing(NewPartition("restricted"));
        ActorCanReach();
        var target = NewTarget();
        target.AddPartition(restricted);
        Existing(target);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetPartitionsCommand(
                target.Guid, new PartitionMembershipsChangeDto(PartitionsToRemove: [restricted.Guid]))));
    }

    [Fact]
    public async Task HandleAsync_WhenPartitionNotFound_ShouldThrow()
    {
        ActorCanReach();
        var target = NewTarget();
        Existing(target);
        var missing = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetPartitionsCommand(
                target.Guid, new PartitionMembershipsChangeDto(PartitionsToAdd: [missing]))));
    }

    [Fact]
    public async Task HandleAsync_WhenSamePartitionIsAddedAndRemoved_ShouldThrow()
    {
        ActorCanReach();
        var partitionGuid = Guid.NewGuid();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetPartitionsCommand(
                Guid.NewGuid(),
                new PartitionMembershipsChangeDto(PartitionsToAdd: [partitionGuid], PartitionsToRemove: [partitionGuid]))));
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsRequested_ShouldThrow()
    {
        ActorCanReach();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetPartitionsCommand(Guid.NewGuid(), new PartitionMembershipsChangeDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFound_ShouldThrow()
    {
        ActorCanReach();
        var missing = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetPartitionsCommand(
                missing, new PartitionMembershipsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToItem_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeItemPartitions], partitionAccesses: [Guid.NewGuid()]);
        var target = NewTarget();
        target.AddPartition(NewPartition("restricted"));
        Existing(target);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetPartitionsCommand(
                target.Guid, new PartitionMembershipsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }
}
