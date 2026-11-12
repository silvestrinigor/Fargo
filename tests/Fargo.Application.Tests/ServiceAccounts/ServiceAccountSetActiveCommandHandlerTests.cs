using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.ServiceAccounts;
using Fargo.Application.Tests.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.ServiceAccounts;

public class ServiceAccountSetActiveCommandHandlerTests
{
    private readonly IServiceAccountRepository serviceAccountRepository = Substitute.For<IServiceAccountRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ServiceAccountSetActiveCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository, serviceAccountRepository),
            serviceAccountRepository,
            currentActor);

    private static ServiceAccount ExistingAccount()
        => Actor.NewServiceAccount(new Nameid("svc"), new PasswordHash("hash")).ServiceAccount!;

    private void Existing(ServiceAccount account)
        => serviceAccountRepository.GetByGuidAsync(account.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(account));

    [Fact]
    public async Task HandleAsync_WhenDeactivating_ShouldDeactivateAndRotateAuthVersion()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateServiceAccount]);
        var account = ExistingAccount();
        var before = account.AuthVersion;
        Existing(account);

        await CreateHandler().HandleAsync(new ServiceAccountSetActiveCommand(account.ActorGuid, false));

        Assert.False(account.Actor.IsActive);
        Assert.NotEqual(before, account.AuthVersion);
    }

    [Fact]
    public async Task HandleAsync_WhenActivating_ShouldActivateWithoutRotatingAuthVersion()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateServiceAccount]);
        var account = ExistingAccount();
        account.Actor.Deactivate();
        var before = account.AuthVersion;
        Existing(account);

        await CreateHandler().HandleAsync(new ServiceAccountSetActiveCommand(account.ActorGuid, true));

        Assert.True(account.Actor.IsActive);
        Assert.Equal(before, account.AuthVersion);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditServiceAccount_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditServiceAccount]);
        var account = ExistingAccount();
        Existing(account);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetActiveCommand(account.ActorGuid, false)));

        Assert.True(account.Actor.IsActive);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateServiceAccount]);
        serviceAccountRepository.GetByGuidAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetActiveCommand(Guid.NewGuid(), false)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToTheAccountPartition_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ActivateServiceAccount]);
        var global = Fargo.Core.Entities.Partitions.Partition.CreateGlobalPartition(new Name("global"));
        var restricted = Fargo.Core.Entities.Partitions.Partition.CreatePartition(new Name("restricted"), global);
        var account = ExistingAccount();
        account.Actor.AddPartition(restricted);
        Existing(account);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetActiveCommand(account.ActorGuid, false)));

        Assert.True(account.Actor.IsActive);
    }
}
