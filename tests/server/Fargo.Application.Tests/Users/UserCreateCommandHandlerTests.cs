using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Application.Users;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.Users;

public class UserCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUserGroupRepository userGroupRepository = Substitute.For<IUserGroupRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IPasswordHasher passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private UserCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new UserService(userRepository),
            userRepository,
            partitionRepository,
            userGroupRepository,
            auditLogRepository,
            currentActor,
            passwordHasher,
            unitOfWork,
            Substitute.For<ILogger<UserCreateCommandHandler>>());

    private static UserCreateCommand SimpleCommand(string nameid = "newuser")
        => new(new UserCreateDto(new Nameid(nameid)));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddUserAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        await CreateHandler().HandleAsync(SimpleCommand());

        userRepository.Received(1).Add(Arg.Any<User>());
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(SimpleCommand()));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(SimpleCommand()));
    }

    [Fact]
    public async Task HandleAsync_WhenNameidAlreadyExists_ShouldThrowDomainValidation()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(SimpleCommand()));

        userRepository.DidNotReceive().Add(Arg.Any<User>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRequestedPartitionNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        var missingPartition = Guid.NewGuid();
        partitionRepository.GetByGuidAsync(missingPartition, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(null));

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), PartitionsToAdd: [missingPartition]));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToRequestedPartition_ShouldThrowAccessDenied()
    {
        // Actor has CreateUser but only global partition access; the requested
        // partition is a different one the actor cannot access.
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateUser]);
        userRepository.ExistsByNameidAsync(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        var global = Partition.CreateGlobalPartition(new Name("global"));
        var partition = Partition.CreatePartition(new Name("restricted"), global);
        partitionRepository.GetByGuidAsync(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Partition?>(partition));

        var command = new UserCreateCommand(
            new UserCreateDto(new Nameid("newuser"), PartitionsToAdd: [partition.Guid]));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }
}
