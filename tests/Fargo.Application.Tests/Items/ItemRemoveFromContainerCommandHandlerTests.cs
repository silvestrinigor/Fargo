using Fargo.Application.Commands.Items;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Tests.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Events;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemRemoveFromContainerCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IItemEventRepository itemEventRepository = Substitute.For<IItemEventRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ItemRemoveFromContainerCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            itemEventRepository,
            currentActor);

    private static Item NewItem()
        => Item.CreateItem(Article.NewArticle(new Name("Widget")));

    private void Existing(Item item)
        => itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

    [Fact]
    public async Task HandleAsync_WhenInContainer_ShouldClearParent()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var item = NewItem();
        Existing(item);
        var box = Item.CreateItem(Article.NewArticleContainer(new Name("Box")));
        item.PlaceInsideContainer(box);

        await CreateHandler().HandleAsync(new ItemRemoveFromContainerCommand(item.Guid));

        Assert.Null(item.ParentItemContainerGuid);
        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(
            e => e!.ItemGuid == item.Guid
                 && e.ItemEventType == ItemEventType.Moviment
                 && e.FromItemContainerGuid == box.Guid
                 && e.ToNoContainer));
    }

    [Fact]
    public async Task HandleAsync_WhenNotInAContainer_ShouldDoNothing()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var item = NewItem();
        Existing(item);

        await CreateHandler().HandleAsync(new ItemRemoveFromContainerCommand(item.Guid));

        Assert.Null(item.ParentItemContainerGuid);
        itemEventRepository.DidNotReceive().Add(Arg.Any<ItemEvent>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorOnlyHasEditItem_ShouldThrowPermissionDenied()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditItem]);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemRemoveFromContainerCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.MoveItem]);
        var missing = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemRemoveFromContainerCommand(missing)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.MoveItem], partitionAccesses: [Guid.NewGuid()]);
        var item = NewItem();
        Existing(item);

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemRemoveFromContainerCommand(item.Guid)));
    }
}
