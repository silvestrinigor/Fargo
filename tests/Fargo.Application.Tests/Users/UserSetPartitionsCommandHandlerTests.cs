using Fargo.Application.Commands.Users;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Users;

public class UserSetPartitionsCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly TestCurrentActor currentActor = new();

    private UserSetPartitionsCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            userRepository,
            partitionRepository,
            currentActor);

    private static User NewTarget()
        => Actor.NewUser(new Nameid("target")).User!;

    private static Partition NewPartition(string name)
        => Partition.CreatePartition(new Name(name), Partition.CreateGlobalPartition(new Name("global")));

    private Partition Existing(Partition partition)
    {
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        return partition;
    }

    private void Existing(User user)
        => userRepository.GetByGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

    private void ActorCanReach(params Partition[] partitions)
        => ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserPartitions],
            [FargoCoreWellKnowGuids.GlobalPartitionGuid, .. partitions.Select(p => p.Guid)]);

    [Fact]
    public async Task HandleAsync_WhenChangingMembershipsAndAccesses_ShouldApplyAll()
    {
        var addMembership = Existing(NewPartition("add-membership"));
        var removeMembership = Existing(NewPartition("remove-membership"));
        var addAccess = Existing(NewPartition("add-access"));
        var removeAccess = Existing(NewPartition("remove-access"));
        ActorCanReach(addMembership, removeMembership, addAccess, removeAccess);
        var target = NewTarget();
        target.Actor.AddPartition(removeMembership);
        target.Actor.AddPartitionAccess(removeAccess);
        Existing(target);

        await CreateHandler().HandleAsync(new UserSetPartitionsCommand(
            target.ActorGuid,
            new PartitionsChangeDto(
                PartitionsToAdd: [addMembership.Guid],
                PartitionsToRemove: [removeMembership.Guid],
                PartitionAccessesToAdd: [addAccess.Guid],
                PartitionAccessesToRemove: [removeAccess.Guid])));

        Assert.Contains(addMembership.Guid, target.Actor.PartitionGuids);
        Assert.DoesNotContain(removeMembership.Guid, target.Actor.PartitionGuids);
        Assert.Contains(target.Actor.PartitionAccesses, a => a.PartitionGuid == addAccess.Guid);
        Assert.DoesNotContain(target.Actor.PartitionAccesses, a => a.PartitionGuid == removeAccess.Guid);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditUser_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUser]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPartitionsCommand(
                target.ActorGuid, new PartitionsChangeDto(PartitionAccessesToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToThePartition_ShouldThrowAccessDenied()
    {
        var restricted = Existing(NewPartition("restricted"));
        ActorCanReach();
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPartitionsCommand(
                target.ActorGuid, new PartitionsChangeDto(PartitionAccessesToAdd: [restricted.Guid]))));

        Assert.Empty(target.Actor.PartitionAccesses);
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
            () => CreateHandler().HandleAsync(new UserSetPartitionsCommand(
                target.ActorGuid, new PartitionsChangeDto(PartitionsToAdd: [missing]))));
    }

    [Fact]
    public async Task HandleAsync_WhenSamePartitionIsAddedAndRemovedAsMembership_ShouldThrow()
    {
        ActorCanReach();
        var partitionGuid = Guid.NewGuid();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPartitionsCommand(
                Guid.NewGuid(),
                new PartitionsChangeDto(PartitionsToAdd: [partitionGuid], PartitionsToRemove: [partitionGuid]))));
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsRequested_ShouldThrow()
    {
        ActorCanReach();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPartitionsCommand(Guid.NewGuid(), new PartitionsChangeDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrow()
    {
        ActorCanReach();
        var missing = Guid.NewGuid();
        userRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPartitionsCommand(
                missing, new PartitionsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToTheUser_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserPartitions], partitionAccesses: [Guid.NewGuid()]);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserSetPartitionsCommand(
                target.ActorGuid, new PartitionsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingTheGlobalPartitionMembership_ShouldThrow()
    {
        var global = Existing(Partition.CreateGlobalPartition(new Name("global")));
        ActorCanReach(global);
        var target = NewTarget();
        Existing(target);

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserSetPartitionsCommand(
                target.ActorGuid, new PartitionsChangeDto(PartitionsToRemove: [global.Guid]))));
    }
}
