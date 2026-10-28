using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Application.Users;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Users;
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
            currentActor,
            unitOfWork);

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldRemoveUserAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteUser]);
        var user = User.CreateUser(new Nameid("target"));
        userRepository.GetByGuidAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        await CreateHandler().HandleAsync(new UserDeleteCommand(user.Guid));

        userRepository.Received(1).Remove(user);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
        var user = User.CreateUser(new Nameid("target"));
        userRepository.GetByGuidAsync(user.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(user));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new UserDeleteCommand(user.Guid)));

        userRepository.DidNotReceive().Remove(Arg.Any<User>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenDeletingAdminUser_ShouldThrowDomainValidation()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteUser]);
        var admin = User.CreateAdministratorUser(new Nameid("admin"));
        userRepository.GetByGuidAsync(admin.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(admin));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new UserDeleteCommand(admin.Guid)));

        userRepository.DidNotReceive().Remove(Arg.Any<User>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
