using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.ServiceAccounts;
using Fargo.Application.Dtos;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.ServiceAccounts;

public class ServiceAccountSetPartitionsCommandHandlerTests
{
    private readonly IServiceAccountRepository serviceAccountRepository = Substitute.For<IServiceAccountRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ServiceAccountSetPartitionsCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository, serviceAccountRepository),
            serviceAccountRepository,
            partitionRepository,
            currentActor);

    private static ServiceAccount NewAccount()
        => Actor.NewServiceAccount(new Nameid("svc"), new PasswordHash("hash")).ServiceAccount!;

    private static Partition NewPartition(string name)
        => Partition.CreatePartition(new Name(name), Partition.CreateGlobalPartition(new Name("global")));

    private Partition Existing(Partition partition)
    {
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        return partition;
    }

    private void Existing(ServiceAccount account)
        => serviceAccountRepository.GetByGuidAsync(account.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(account));

    private void ActorCanReach(params Partition[] partitions)
        => ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeServiceAccountPartitions],
            [FargoCoreWellKnowGuids.GlobalPartitionGuid, .. partitions.Select(p => p.Guid)]);

    [Fact]
    public async Task HandleAsync_WhenChangingMembershipsAndAccesses_ShouldApplyAll()
    {
        var addMembership = Existing(NewPartition("add-membership"));
        var removeMembership = Existing(NewPartition("remove-membership"));
        var addAccess = Existing(NewPartition("add-access"));
        var removeAccess = Existing(NewPartition("remove-access"));
        ActorCanReach(addMembership, removeMembership, addAccess, removeAccess);
        var account = NewAccount();
        account.Actor.AddPartition(removeMembership);
        account.Actor.AddPartitionAccess(removeAccess);
        Existing(account);

        await CreateHandler().HandleAsync(new ServiceAccountSetPartitionsCommand(
            account.ActorGuid,
            new PartitionsChangeDto(
                PartitionsToAdd: [addMembership.Guid],
                PartitionsToRemove: [removeMembership.Guid],
                PartitionAccessesToAdd: [addAccess.Guid],
                PartitionAccessesToRemove: [removeAccess.Guid])));

        Assert.Contains(addMembership.Guid, account.Actor.PartitionGuids);
        Assert.DoesNotContain(removeMembership.Guid, account.Actor.PartitionGuids);
        Assert.Contains(account.Actor.PartitionAccesses, a => a.PartitionGuid == addAccess.Guid);
        Assert.DoesNotContain(account.Actor.PartitionAccesses, a => a.PartitionGuid == removeAccess.Guid);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditServiceAccount_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditServiceAccount]);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPartitionsCommand(
                Guid.NewGuid(), new PartitionsChangeDto(PartitionAccessesToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToThePartition_ShouldThrowAccessDenied()
    {
        var restricted = Existing(NewPartition("restricted"));
        ActorCanReach();
        var account = NewAccount();
        Existing(account);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPartitionsCommand(
                account.ActorGuid, new PartitionsChangeDto(PartitionsToAdd: [restricted.Guid]))));

        Assert.Empty(account.Actor.PartitionGuids);
    }

    [Fact]
    public async Task HandleAsync_WhenPartitionNotFound_ShouldThrow()
    {
        ActorCanReach();
        var account = NewAccount();
        Existing(account);
        var missing = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPartitionsCommand(
                account.ActorGuid, new PartitionsChangeDto(PartitionsToAdd: [missing]))));
    }

    [Fact]
    public async Task HandleAsync_WhenSamePartitionIsAddedAndRemovedAsMembership_ShouldThrow()
    {
        ActorCanReach();
        var partitionGuid = Guid.NewGuid();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPartitionsCommand(
                Guid.NewGuid(),
                new PartitionsChangeDto(PartitionsToAdd: [partitionGuid], PartitionsToRemove: [partitionGuid]))));
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsRequested_ShouldThrow()
    {
        ActorCanReach();

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPartitionsCommand(Guid.NewGuid(), new PartitionsChangeDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenAccountNotFound_ShouldThrow()
    {
        ActorCanReach();
        serviceAccountRepository.GetByGuidAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPartitionsCommand(
                Guid.NewGuid(), new PartitionsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToTheAccount_ShouldThrowAccessDenied()
    {
        ActorCanReach();
        var account = NewAccount();
        account.Actor.AddPartition(NewPartition("restricted"));
        Existing(account);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPartitionsCommand(
                account.ActorGuid, new PartitionsChangeDto(PartitionsToAdd: [Guid.NewGuid()]))));
    }
}
