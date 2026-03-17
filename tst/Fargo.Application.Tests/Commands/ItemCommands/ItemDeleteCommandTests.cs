using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.ItemCommands;

public sealed class ItemDeleteCommandHandlerTests
{
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly ItemDeleteCommandHandler handler;

    public ItemDeleteCommandHandlerTests()
    {
        handler = new ItemDeleteCommandHandler(
            itemRepository,
            userRepository,
            unitOfWork,
            currentUser);
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

        await itemRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        itemRepository.DidNotReceive()
            .Remove(Arg.Any<Item>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowItemNotFoundFargoApplicationException_When_ItemIsNotFound()
    {
        // Arrange
        var actor = CreateUser();
        actor.AddPermission(ActionType.DeleteItem);
        var itemGuid = Guid.NewGuid();
        var command = CreateCommand(itemGuid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        itemRepository
            .GetByGuid(itemGuid, Arg.Any<CancellationToken>())
            .Returns((Item?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<ItemNotFoundFargoApplicationException>(act);

        itemRepository.DidNotReceive()
            .Remove(Arg.Any<Item>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var actor = CreateUser();
        var item = CreateItem();
        var command = CreateCommand(item.Guid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureItemLookup(item);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        itemRepository.DidNotReceive()
            .Remove(Arg.Any<Item>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveItemAndSaveChanges_When_RequestIsValid()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.DeleteItem);
        var item = CreateItem();
        var command = CreateCommand(item.Guid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureItemLookup(item);

        // Act
        await handler.Handle(command);

        // Assert
        itemRepository.Received(1)
            .Remove(item);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.DeleteItem);
        var item = CreateItem();
        var cancellationToken = new CancellationTokenSource().Token;
        var command = CreateCommand(item.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        itemRepository
            .GetByGuid(item.Guid, cancellationToken)
            .Returns(item);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, cancellationToken);

        await itemRepository.Received(1)
            .GetByGuid(item.Guid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ThrowUserInactiveFargoDomainException_When_ActorIsInactive()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.DeleteItem);
        actor.IsActive = false;

        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        var exception = await Assert.ThrowsAsync<UserInactiveFargoDomainException>(act);
        Assert.Equal(actor.Guid, exception.UserGuid);

        await itemRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        itemRepository.DidNotReceive()
            .Remove(Arg.Any<Item>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
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

    private void ConfigureItemLookup(Item item)
    {
        itemRepository
            .GetByGuid(item.Guid, Arg.Any<CancellationToken>())
            .Returns(item);
    }

    private static ItemDeleteCommand CreateCommand(Guid? itemGuid = null)
        => new(itemGuid ?? Guid.NewGuid());

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("user123"),
            PasswordHash = new PasswordHash(new string('a', PasswordHash.MinLength))
        };
    }

    private static User CreateUserWithPermission(ActionType action)
    {
        var user = CreateUser();
        user.AddPermission(action);
        return user;
    }

    private static Item CreateItem()
    {
        return new Item
        {
            Article = new Article
            {
                Name = new Name("Article")
            },
            Guid = Guid.NewGuid()
        };
    }
}
