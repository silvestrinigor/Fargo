using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.PartitionCommands;

public sealed class PartitionCreateCommandHandlerTests
{
    private static readonly Name partitionName = new("Administrators");
    private static readonly Description partitionDescription =
        new("Partition for administrative access");

    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly PartitionCreateCommandHandler handler;

    public PartitionCreateCommandHandlerTests()
    {
        handler = new PartitionCreateCommandHandler(
            userRepository,
            partitionRepository,
            currentUser,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository
            .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        partitionRepository.DidNotReceive().Add(Arg.Any<Partition>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsInactive()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreatePartition);
        actor.IsActive = false;

        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        partitionRepository.DidNotReceive().Add(Arg.Any<Partition>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var actor = CreateUser();
        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        partitionRepository.DidNotReceive().Add(Arg.Any<Partition>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AddPartitionAndSaveChanges_When_ActorHasPermission()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreatePartition);
        var command = CreateCommand(partitionDescription);
        Partition? addedPartition = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        CaptureAddedPartition(partition => addedPartition = partition);

        // Act
        var result = await handler.Handle(command);

        // Assert
        partitionRepository.Received(1).Add(Arg.Any<Partition>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedPartition);
        Assert.Equal(command.Name, addedPartition!.Name);
        Assert.Equal(command.Description, addedPartition.Description);
        Assert.Equal(addedPartition.Guid, result);
    }

    [Fact]
    public async Task Handle_Should_UseDescriptionEmpty_When_DescriptionIsNull()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreatePartition);
        var command = CreateCommand(description: null);
        Partition? addedPartition = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        CaptureAddedPartition(partition => addedPartition = partition);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedPartition);
        Assert.Equal(Description.Empty, addedPartition!.Description);
    }

    [Fact]
    public async Task Handle_Should_UseCurrentUserGuidToLoadActor()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreatePartition);
        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        await handler.Handle(command);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreatePartition);
        var command = CreateCommand();
        var cancellationToken = new CancellationTokenSource().Token;

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, cancellationToken);

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

    private void CaptureAddedPartition(Action<Partition> capture)
    {
        partitionRepository
            .When(x => x.Add(Arg.Any<Partition>()))
            .Do(callInfo => capture(callInfo.Arg<Partition>()));
    }

    private static PartitionCreateCommand CreateCommand(
        Description? description = null)
    {
        return new PartitionCreateCommand(
            partitionName,
            description);
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
}
