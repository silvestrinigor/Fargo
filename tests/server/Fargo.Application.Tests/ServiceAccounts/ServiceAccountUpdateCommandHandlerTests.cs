using Fargo.Application.ServiceAccounts;
using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Informations;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Security;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using NSubstitute;

namespace Fargo.Application.Tests.ServiceAccounts;

public class ServiceAccountUpdateCommandHandlerTests
{
    private readonly IServiceAccountRepository serviceAccountRepository = Substitute.For<IServiceAccountRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ServiceAccountUpdateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository, serviceAccountRepository),
            serviceAccountRepository,
            partitionRepository,
            auditLogRepository,
            currentActor);

    private static ServiceAccount ExistingAccount()
        => Actor.NewServiceAccount(new Nameid("svc"), new PasswordHash("hash")).ServiceAccount!;

    private void Existing(ServiceAccount account)
        => serviceAccountRepository.GetByGuidAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(account));

    [Fact]
    public async Task HandleAsync_WhenAddingMembership_ShouldAddPartitionMembership()
    {
        var account = ExistingAccount();
        Existing(account);

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var partition = Partition.CreatePartition(new Name("branch"), global);

        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditServiceAccount], [partition.Guid]);

        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        var command = new ServiceAccountUpdateCommand(
            account.ActorGuid, new ServiceAccountUpdateDto(PartitionsToAdd: [partition.Guid]));

        await CreateHandler().HandleAsync(command);

        Assert.Contains(partition.Guid, account.Actor.PartitionGuids);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingMembership_ShouldRemovePartitionMembership()
    {
        var global = Partition.CreateGlobalPartition(new Name("global"));
        var partition = Partition.CreatePartition(new Name("branch"), global);

        var account = ExistingAccount();
        account.Actor.AddPartition(partition);
        Existing(account);

        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditServiceAccount], [partition.Guid]);

        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        var command = new ServiceAccountUpdateCommand(
            account.ActorGuid, new ServiceAccountUpdateDto(PartitionsToRemove: [partition.Guid]));

        await CreateHandler().HandleAsync(command);

        Assert.DoesNotContain(partition.Guid, account.Actor.PartitionGuids);
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToMembershipPartition_ShouldThrowAccessDenied()
    {
        var account = ExistingAccount();
        Existing(account);

        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditServiceAccount]);

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var restricted = Partition.CreatePartition(new Name("restricted"), global);
        partitionRepository.GetByGuidAsync(restricted.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(restricted));

        var command = new ServiceAccountUpdateCommand(
            account.ActorGuid, new ServiceAccountUpdateDto(PartitionsToAdd: [restricted.Guid]));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenAccountNotFound_ShouldThrow()
    {
        serviceAccountRepository.GetByGuidAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(null));

        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditServiceAccount]);

        var command = new ServiceAccountUpdateCommand(
            Guid.NewGuid(), new ServiceAccountUpdateDto(Description: "x"));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        var command = new ServiceAccountUpdateCommand(
            Guid.NewGuid(), new ServiceAccountUpdateDto(Description: "x"));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }
}
