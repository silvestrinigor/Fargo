using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.PartitionCommands;

public sealed class PartitionUpdateCommandHandlerTests
{
    private static readonly Name updatedName = new("Updated Partition");
    private static readonly Description updatedDescription =
        new("Updated partition description");

    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly PartitionService partitionService;
    private readonly PartitionUpdateCommandHandler handler;

    public PartitionUpdateCommandHandlerTests()
    {
        partitionService = new PartitionService(partitionRepository);

        handler = new PartitionUpdateCommandHandler(
            partitionService,
            userRepository,
            currentUser,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var command = CreateCommand(Guid.NewGuid());

        currentUser.UserGuid.Returns(actorGuid);

        userRepository
            .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsInactive()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditPartition);
        actor.IsActive = false;

        var command = CreateCommand(Guid.NewGuid());

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var actor = CreateUser();
        var command = CreateCommand(Guid.NewGuid());

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowPartitionNotFoundFargoApplicationException_When_PartitionDoesNotExist()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditPartition);
        var partitionGuid = Guid.NewGuid();
        var command = CreateCommand(partitionGuid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        partitionRepository
            .GetByGuid(partitionGuid, Arg.Any<CancellationToken>())
            .Returns((Partition?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        var exception = await Assert.ThrowsAsync<PartitionNotFoundFargoApplicationException>(act);
        Assert.Equal(partitionGuid, exception.PartitionGuid);

        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowPartitionNotFoundFargoApplicationException_When_ActorDoesNotHaveAccessToPartition()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditPartition);
        var partition = CreatePartition();
        var command = CreateCommand(partition.Guid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        partitionRepository
            .GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        var exception = await Assert.ThrowsAsync<PartitionNotFoundFargoApplicationException>(act);
        Assert.Equal(partition.Guid, exception.PartitionGuid);

        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdatePartitionAndSaveChanges_When_ActorHasPermissionAndAccess()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditPartition);
        var partition = CreatePartition();
        var command = CreateCommand(
            partition.Guid,
            updatedName,
            updatedDescription);

        actor.AddPartitionAccess(partition);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        partitionRepository
            .GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(updatedDescription, partition.Description);

        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseDescriptionEmpty_When_DescriptionIsNull()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditPartition);
        var partition = CreatePartition(description: new Description("Old description"));
        var command = CreateCommand(
            partition.Guid,
            updatedName,
            description: null);

        actor.AddPartitionAccess(partition);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        partitionRepository
            .GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(Description.Empty, partition.Description);

        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseCurrentUserGuidToLoadActor()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditPartition);
        var command = CreateCommand(Guid.NewGuid());

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        partitionRepository
            .GetByGuid(command.PartitionGuid, Arg.Any<CancellationToken>())
            .Returns((Partition?)null);

        // Act
        await Assert.ThrowsAsync<PartitionNotFoundFargoApplicationException>(
            () => handler.Handle(command));

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditPartition);
        var partition = CreatePartition();
        var command = CreateCommand(partition.Guid);
        var cancellationToken = new CancellationTokenSource().Token;

        actor.AddPartitionAccess(partition);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        partitionRepository
            .GetByGuid(partition.Guid, cancellationToken)
            .Returns(partition);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, cancellationToken);

        await partitionRepository.Received(1)
            .GetByGuid(partition.Guid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    private void ConfigureCurrentUser(User actor)
    {
        currentUser.UserGuid.Returns(actor.Guid);
    }

    private void ConfigureActorLookup(User actor)
    {
        userRepository
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);
    }

    private static PartitionUpdateCommand CreateCommand(
        Guid partitionGuid,
        Name? name = null,
        Description? description = null)
    {
        return new PartitionUpdateCommand(
            partitionGuid,
            new(description));
    }

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("igor123"),
            PasswordHash = new PasswordHash(new string('a', PasswordHash.MinLength))
        };
    }

    private static User CreateUserWithPermission(ActionType action)
    {
        var user = CreateUser();
        user.AddPermission(action);
        return user;
    }

    private static Partition CreatePartition(
        Guid? guid = null,
        Name? name = null,
        Description? description = null)
    {
        return new Partition
        {
            Guid = guid ?? Guid.NewGuid(),
            Name = name ?? new Name(new string('a', Name.MinLength)),
            Description = description ?? new Description(new string('a', Description.MinLength)),
            IsActive = true
        };
    }
}