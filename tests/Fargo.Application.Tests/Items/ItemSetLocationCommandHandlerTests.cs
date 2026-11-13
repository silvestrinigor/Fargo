using Fargo.Application.Commands.Items;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemSetLocationCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ItemSetLocationCommandHandler CreateHandler()
        => new(
            new ItemService(itemRepository),
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            currentActor);

    private static Item NewItem()
        => Item.CreateItem(Article.NewArticle(new Name("Widget")));

    private static Item NewContainer(string name)
        => Item.CreateItem(Article.NewArticleContainer(new Name(name)));

    private void Existing(Item item)
        => itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldPlaceItemInContainerAndRecordMoviment()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var item = NewItem();
        Existing(item);
        var parentContainer = NewContainer("Box");
        Existing(parentContainer);
        itemRepository.GetContainedDescendantGuidsAsync(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        await CreateHandler().HandleAsync(new ItemSetLocationCommand(item.Guid, parentContainer.Guid));

        Assert.Equal(parentContainer.Guid, item.ParentItemContainerGuid);
        Assert.Single(item.Moviments);
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyInTheContainer_ShouldDoNothing()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var item = NewItem();
        Existing(item);
        var parentContainer = NewContainer("Box");
        Existing(parentContainer);
        item.PlaceInsideContainer(parentContainer);
        var movimentsBefore = item.Moviments.Count;

        await CreateHandler().HandleAsync(new ItemSetLocationCommand(item.Guid, parentContainer.Guid));

        Assert.Equal(movimentsBefore, item.Moviments.Count);
        await itemRepository.DidNotReceive().AcquireContainmentLockAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMovingContainer_ShouldTakeContainmentLock()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var item = NewContainer("Crate");
        Existing(item);
        var parentContainer = NewContainer("Box");
        Existing(parentContainer);
        itemRepository.GetContainedDescendantGuidsAsync(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        await CreateHandler().HandleAsync(new ItemSetLocationCommand(item.Guid, parentContainer.Guid));

        await itemRepository.Received(1).AcquireContainmentLockAsync(Arg.Any<CancellationToken>());
        Assert.Equal(parentContainer.Guid, item.ParentItemContainerGuid);
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditItem_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditItem]);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetLocationCommand(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetLocationCommand(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var missing = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetLocationCommand(missing, Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenDestinationNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var item = NewItem();
        Existing(item);
        var missing = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetLocationCommand(item.Guid, missing)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.MoveItem], partitionAccesses: [Guid.NewGuid()]);
        var item = NewItem();
        Existing(item);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemSetLocationCommand(item.Guid, Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenParentContainerWouldCreateCircularHierarchy_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var item = NewContainer("Crate");
        Existing(item);
        var parentContainer = NewContainer("Box");
        Existing(parentContainer);

        // Parent appears among the item's descendants -> circular assignment.
        itemRepository.GetContainedDescendantGuidsAsync(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([parentContainer.Guid]));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new ItemSetLocationCommand(item.Guid, parentContainer.Guid)));

        Assert.Null(item.ParentItemContainerGuid);
    }
}
