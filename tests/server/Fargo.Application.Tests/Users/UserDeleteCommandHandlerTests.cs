using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Application.Users;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Users;

public class UserDeleteCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private UserDeleteCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            userRepository,
            auditLogRepository,
            currentActor);

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldRemoveUserAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteUser]);
        var user = Actor.NewUser(new Nameid("target")).User!;
        userRepository.GetByGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        await CreateHandler().HandleAsync(new UserDeleteCommand(user.ActorGuid));

        userRepository.Received(1).Remove(user);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteUser]);
        var missing = Guid.NewGuid();
        userRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserDeleteCommand(missing)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.DeleteUser], partitionAccesses: [Guid.NewGuid()]);
        var user = Actor.NewUser(new Nameid("target")).User!;
        userRepository.GetByGuidAsync(user.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserDeleteCommand(user.ActorGuid)));

        userRepository.DidNotReceive().Remove(Arg.Any<User>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingAdminUser_ShouldThrowDomainValidation()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteUser]);
        var admin = Actor.NewAdministratorUser(new Nameid("admin")).User!;
        userRepository.GetByGuidAsync(admin.ActorGuid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(admin));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserDeleteCommand(admin.ActorGuid)));

        userRepository.DidNotReceive().Remove(Arg.Any<User>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
