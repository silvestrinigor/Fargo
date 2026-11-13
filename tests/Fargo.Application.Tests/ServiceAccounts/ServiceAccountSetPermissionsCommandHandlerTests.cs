using Fargo.Application.Commands.ServiceAccounts;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
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

public class ServiceAccountSetPermissionsCommandHandlerTests
{
    private readonly IServiceAccountRepository serviceAccountRepository = Substitute.For<IServiceAccountRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ServiceAccountSetPermissionsCommandHandler CreateHandler(ICurrentActor? actor = null)
        => new(
            ActorTestFactory.Resolver(userRepository, serviceAccountRepository),
            serviceAccountRepository,
            actor ?? currentActor);

    private static ServiceAccount NewAccount()
        => Actor.NewServiceAccount(new Nameid("svc"), new PasswordHash("hash")).ServiceAccount!;

    private void Existing(ServiceAccount account)
        => serviceAccountRepository.GetByGuidAsync(account.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(account));

    [Fact]
    public async Task HandleAsync_WhenGrantingAndRevoking_ShouldApplyBoth()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid,
            [ActionType.ChangeServiceAccountPermissions, ActionType.CreateItem, ActionType.DeleteItem]);
        var account = NewAccount();
        account.Actor.AddPermission(ActionType.DeleteItem);
        Existing(account);

        await CreateHandler().HandleAsync(new ServiceAccountSetPermissionsCommand(
            account.ActorGuid,
            new PermissionsChangeDto(
                PermissionsToAdd: [ActionType.CreateItem],
                PermissionsToRemove: [ActionType.DeleteItem])));

        Assert.Contains(ActionType.CreateItem, account.Actor.Permissions);
        Assert.DoesNotContain(ActionType.DeleteItem, account.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditServiceAccount_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditServiceAccount, ActionType.CreateItem]);
        var account = NewAccount();
        Existing(account);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPermissionsCommand(
                account.ActorGuid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));

        Assert.Empty(account.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenGrantingAPermissionTheActorDoesNotHold_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeServiceAccountPermissions]);
        var account = NewAccount();
        Existing(account);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPermissionsCommand(
                account.ActorGuid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.DeleteItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenRevokingAPermissionTheActorDoesNotHold_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeServiceAccountPermissions]);
        var account = NewAccount();
        account.Actor.AddPermission(ActionType.DeleteItem);
        Existing(account);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPermissionsCommand(
                account.ActorGuid, new PermissionsChangeDto(PermissionsToRemove: [ActionType.DeleteItem]))));

        Assert.Contains(ActionType.DeleteItem, account.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenSamePermissionIsAddedAndRemoved_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeServiceAccountPermissions, ActionType.CreateItem]);
        var account = NewAccount();
        Existing(account);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPermissionsCommand(
                account.ActorGuid,
                new PermissionsChangeDto(
                    PermissionsToAdd: [ActionType.CreateItem],
                    PermissionsToRemove: [ActionType.CreateItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenNothingIsRequested_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ChangeServiceAccountPermissions]);
        var account = NewAccount();
        Existing(account);

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPermissionsCommand(account.ActorGuid, new PermissionsChangeDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenServiceAccountChangesItsOwnPermissions_ShouldThrow()
    {
        var account = NewAccount();
        account.Actor.AddPermission(ActionType.ChangeServiceAccountPermissions);
        account.Actor.AddPermission(ActionType.CreateItem);
        Existing(account);
        serviceAccountRepository.GetAllActivePartitionAccessGuidsAsync(account.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([FargoCoreWellKnowGuids.GlobalPartitionGuid]));
        var self = new TestCurrentActor { Guid = account.ActorGuid, ActorType = ActorType.Service };

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler(self).HandleAsync(new ServiceAccountSetPermissionsCommand(
                account.ActorGuid, new PermissionsChangeDto(PermissionsToRemove: [ActionType.CreateItem]))));

        Assert.Contains(ActionType.CreateItem, account.Actor.Permissions);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeServiceAccountPermissions, ActionType.CreateItem]);
        serviceAccountRepository.GetByGuidAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ServiceAccount?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPermissionsCommand(
                Guid.NewGuid(), new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToTheAccountPartition_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.ChangeServiceAccountPermissions, ActionType.CreateItem]);
        var global = Partition.CreateGlobalPartition(new Name("global"));
        var restricted = Partition.CreatePartition(new Name("restricted"), global);
        var account = NewAccount();
        account.Actor.AddPartition(restricted);
        Existing(account);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ServiceAccountSetPermissionsCommand(
                account.ActorGuid, new PermissionsChangeDto(PermissionsToAdd: [ActionType.CreateItem]))));
    }
}
