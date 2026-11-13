using Fargo.Application.Commands.UserGroups;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.UserGroups;

public class UserGroupSetPartitionsCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly TestCurrentActor currentActor = new();

    private UserGroupSetPartitionsCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            userGroupRepository,
            partitionRepository,
            currentActor);

    private static UserGroup NewGroup()
        => UserGroup.CreateUserGroup(new Nameid("editors"));

    private static Partition NewPartition(string name)
        => Partition.CreatePartition(new Name(name), Partition.CreateGlobalPartition(new Name("global")));

    private Partition Existing(Partition partition)
    {
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        return partition;
    }

    private void Existing(UserGroup group)
        => userGroupRepository.GetByGuidAsync(group.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(group));

    private void ActorCanReach(params Partition[] partitions)
        => ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupPartitions],
            [FargoCoreWellKnowGuids.GlobalPartitionGuid, .. partitions.Select(p => p.Guid)]);

    [Fact]
    public async Task HandleAsync_WhenChangingMembershipsAndAccesses_ShouldApplyAll()
    {
        var addMembership = Existing(NewPartition("add-membership"));
        var removeMembership = Existing(NewPartition("remove-membership"));
        var addAccess = Existing(NewPartition("add-access"));
        var removeAccess = Existing(NewPartition("remove-access"));
        ActorCanReach(addMembership, removeMembership, addAccess, removeAccess);
        var group = NewGroup();
        group.AddPartition(removeMembership);
        group.AddPartitionAccess(removeAccess);
        Existing(group);

        await CreateHandler().HandleAsync(new UserGroupSetPartitionsCommand(
            group.Guid,
            new PartitionsChangeDto(
                PartitionsToAdd: [addMembership.Guid],
                PartitionsToRemove: [removeMembership.Guid],
                PartitionAccessesToAdd: [addAccess.Guid],
                PartitionAccessesToRemove: [removeAccess.Guid])));

        Assert.Contains(addMembership.Guid, group.PartitionGuids);
        Assert.DoesNotContain(removeMembership.Guid, group.PartitionGuids);
        Assert.Contains(group.PartitionAccesses, a => a.PartitionGuid == addAccess.Guid);
        Assert.DoesNotContain(group.PartitionAccesses, a => a.PartitionGuid == removeAccess.Guid);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditUserGroup_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditUserGroup]);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPartitionsCommand(
                Guid.NewGuid(), new PartitionsChangeDto(PartitionAccessesToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToThePartition_ShouldThrowAccessDenied()
    {
        var restricted = Existing(NewPartition("restricted"));
        ActorCanReach();
        var group = NewGroup();
        Existing(group);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPartitionsCommand(
                group.Guid, new PartitionsChangeDto(PartitionAccessesToAdd: [restricted.Guid]))));

        Assert.Empty(group.PartitionAccesses);
    }

    [Fact]
    public async Task HandleAsync_WhenSamePartitionIsAddedAndRemovedAsAccess_ShouldThrow()
    {
        ActorCanReach();
        var partitionGuid = Guid.NewGuid();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPartitionsCommand(
                Guid.NewGuid(),
                new PartitionsChangeDto(
                    PartitionAccessesToAdd: [partitionGuid], PartitionAccessesToRemove: [partitionGuid]))));
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsRequested_ShouldThrow()
    {
        ActorCanReach();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPartitionsCommand(Guid.NewGuid(), new PartitionsChangeDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenUserGroupNotFound_ShouldThrow()
    {
        ActorCanReach();
        var missing = Guid.NewGuid();
        userGroupRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserGroup?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPartitionsCommand(
                missing, new PartitionsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToTheGroup_ShouldThrowAccessDenied()
    {
        // Groups always belong to the global partition, so an actor without access to it cannot reach the group.
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeUserGroupPartitions], partitionAccesses: [Guid.NewGuid()]);
        var group = NewGroup();
        Existing(group);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserGroupSetPartitionsCommand(
                group.Guid, new PartitionsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }
}
