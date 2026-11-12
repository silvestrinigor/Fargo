using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.ServiceAccounts;
using Fargo.Application.Tests.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.ServiceAccounts;

public class ServiceAccountRotateSecretCommandHandlerTests
{
    private readonly IServiceAccountRepository serviceAccountRepository = Substitute.For<IServiceAccountRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IClientSecretGenerator secretGenerator = Substitute.For<IClientSecretGenerator>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly TestCurrentActor currentActor = new();

    public ServiceAccountRotateSecretCommandHandlerTests()
    {
        secretGenerator.Generate().Returns(new Password("Secret-Value-1234"));
        passwordHasher.Hash(Arg.Any<Password>()).Returns(new PasswordHash("rotated-hash"));
    }

    private ServiceAccountRotateSecretCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository, serviceAccountRepository),
            serviceAccountRepository,
            secretGenerator,
            passwordHasher,
            currentActor);

    private static ServiceAccount NewAccount()
        => Actor.NewServiceAccount(new Nameid("svc"), new PasswordHash("hash")).ServiceAccount!;

    private void Existing(ServiceAccount account)
        => serviceAccountRepository.GetByGuidAsync(account.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(account));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldRotateTheSecretAndReturnItOnce()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.RotateServiceAccountSecret]);
        var account = NewAccount();
        Existing(account);

        var result = await CreateHandler().HandleAsync(new ServiceAccountRotateSecretCommand(account.ActorGuid));

        Assert.Equal("Secret-Value-1234", result.ClientSecret);
        Assert.Equal("rotated-hash", account.SecretHash.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditServiceAccount_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditServiceAccount]);
        var account = NewAccount();
        Existing(account);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountRotateSecretCommand(account.ActorGuid)));

        Assert.Equal("hash", account.SecretHash.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.RotateServiceAccountSecret]);
        serviceAccountRepository.GetByGuidAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountRotateSecretCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToTheAccount_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.RotateServiceAccountSecret]);
        var account = NewAccount();
        account.Actor.AddPartition(
            Partition.CreatePartition(new Name("restricted"), Partition.CreateGlobalPartition(new Name("global"))));
        Existing(account);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountRotateSecretCommand(account.ActorGuid)));

        Assert.Equal("hash", account.SecretHash.Value);
    }
}