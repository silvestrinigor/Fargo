using Fargo.Application.ServiceAccounts;
using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.ServiceAccounts;
using Fargo.Core.Users;
using NSubstitute;

namespace Fargo.Application.Tests.ServiceAccounts;

public class ServiceAccountCreateCommandHandlerTests
{
    private readonly IServiceAccountRepository serviceAccountRepository = Substitute.For<IServiceAccountRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IClientSecretGenerator secretGenerator = Substitute.For<IClientSecretGenerator>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly TestCurrentActor currentActor = new();

    public ServiceAccountCreateCommandHandlerTests()
    {
        secretGenerator.Generate().Returns(new Password("Secret-Value-1234"));
        passwordHasher.Hash(Arg.Any<Password>()).Returns(new PasswordHash("hash"));
    }

    private ServiceAccountCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository, serviceAccountRepository),
            serviceAccountRepository,
            partitionRepository,
            auditLogRepository,
            secretGenerator,
            passwordHasher,
            currentActor);

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddAccountAndAudit()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateServiceAccount]);

        var command = new ServiceAccountCreateCommand(new ServiceAccountCreateDto("svc"));

        await CreateHandler().HandleAsync(command);

        serviceAccountRepository.Received(1).Add(Arg.Any<ServiceAccount>());
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        var command = new ServiceAccountCreateCommand(new ServiceAccountCreateDto("svc"));

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenPartitionMembershipRequested_ShouldAssignMembership()
    {
        var global = Partition.CreateGlobalPartition(new Fargo.Core.Informations.Name("global"));
        var partition = Partition.CreatePartition(new Fargo.Core.Informations.Name("branch"), global);

        // Actor has CreateServiceAccount and access to the requested partition.
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.CreateServiceAccount], [partition.Guid]);

        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        ServiceAccount? added = null;
        serviceAccountRepository.When(r => r.Add(Arg.Any<ServiceAccount>()))
            .Do(ci => added = ci.Arg<ServiceAccount>());

        var command = new ServiceAccountCreateCommand(
            new ServiceAccountCreateDto("svc", PartitionsToAdd: [partition.Guid]));

        await CreateHandler().HandleAsync(command);

        Assert.NotNull(added);
        Assert.Contains(partition.Guid, added!.PartitionGuids);
    }

    [Fact]
    public async Task HandleAsync_WhenMembershipPartitionNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateServiceAccount]);

        var missing = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        var command = new ServiceAccountCreateCommand(
            new ServiceAccountCreateDto("svc", PartitionsToAdd: [missing]));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToMembershipPartition_ShouldThrowAccessDenied()
    {
        // Actor has the permission but only global access; the requested
        // membership partition is a restricted one it cannot reach.
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateServiceAccount]);

        var global = Partition.CreateGlobalPartition(new Fargo.Core.Informations.Name("global"));
        var restricted = Partition.CreatePartition(new Fargo.Core.Informations.Name("restricted"), global);
        partitionRepository.GetByGuidAsync(restricted.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(restricted));

        var command = new ServiceAccountCreateCommand(
            new ServiceAccountCreateDto("svc", PartitionsToAdd: [restricted.Guid]));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }
}
