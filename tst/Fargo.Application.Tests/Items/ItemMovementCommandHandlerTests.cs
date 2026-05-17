using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public sealed class ItemMovementCommandHandlerTests
{
    [Fact]
    public async Task SetParentContainer_Should_RecordMovement_WhenParentChanges()
    {
        var item = CreateItem();
        var parent = CreateContainerItem();
        var itemRepository = CreateItemRepository(item, parent);
        itemRepository
            .GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);
        var movementRepository = Substitute.For<IItemMovementRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemSetParentContainerCommandHandler(
            itemRepository,
            movementRepository,
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ItemSetParentContainerCommandHandler>>());

        await handler.Handle(new ItemSetParentContainerCommand(item.Guid, parent.Guid));

        movementRepository.Received(1).Add(Arg.Is<ItemMovement>(movement =>
            movement.ItemGuid == item.Guid &&
            movement.FromParentContainerGuid == null &&
            movement.ToParentContainerGuid == parent.Guid &&
            movement.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task SetParentContainer_Should_NotRecordMovement_WhenParentIsUnchanged()
    {
        var item = CreateItem();
        var parent = CreateContainerItem();
        var itemRepository = CreateItemRepository(item, parent);
        itemRepository
            .GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);
        await new ItemService(itemRepository).MoveToContainer(parent, item);
        var movementRepository = Substitute.For<IItemMovementRepository>();
        var handler = new ItemSetParentContainerCommandHandler(
            itemRepository,
            movementRepository,
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditItem)),
            Substitute.For<ILogger<ItemSetParentContainerCommandHandler>>());

        await handler.Handle(new ItemSetParentContainerCommand(item.Guid, parent.Guid));

        movementRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task SetParentContainer_Should_RecordMovement_WhenItemIsRemovedFromContainer()
    {
        var item = CreateItem();
        var parent = CreateContainerItem();
        var itemRepository = CreateItemRepository(item, parent);
        itemRepository
            .GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);
        await new ItemService(itemRepository).MoveToContainer(parent, item);
        var movementRepository = Substitute.For<IItemMovementRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemSetParentContainerCommandHandler(
            itemRepository,
            movementRepository,
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ItemSetParentContainerCommandHandler>>());

        await handler.Handle(new ItemSetParentContainerCommand(item.Guid, null));

        movementRepository.Received(1).Add(Arg.Is<ItemMovement>(movement =>
            movement.ItemGuid == item.Guid &&
            movement.FromParentContainerGuid == parent.Guid &&
            movement.ToParentContainerGuid == null &&
            movement.ActorGuid == actor.ActorGuid));
        Assert.Null(item.ParentContainerGuid);
    }

    private static IItemRepository CreateItemRepository(params Item[] items)
    {
        var repository = Substitute.For<IItemRepository>();
        foreach (var item in items)
        {
            repository.GetByGuid(item.Guid, Arg.Any<CancellationToken>())
                .Returns(item);
        }

        return repository;
    }

    private static ICurrentAuthorizationContext CreateCurrentAuthorizationContext(IAuthorizationContext actor)
    {
        var currentAuthorizationContext = Substitute.For<ICurrentAuthorizationContext>();
        currentAuthorizationContext.GetAsync(Arg.Any<CancellationToken>())
            .Returns(actor);

        return currentAuthorizationContext;
    }

    private static IAuthorizationContext CreateActor(params ActionType[] permissions)
        => new AuthorizationContext(
            Guid.NewGuid(),
            IsAuthenticated: true,
            IsAdmin: false,
            permissions,
            PartitionAccesses: [],
            UserGroupGuids: []);

    private static Item CreateItem()
        => new(Article.CreateArticle(new Name("Article")));

    private static Item CreateContainerItem()
        => new(Article.CreateArticleContainer(new Name("Container article"), null));
}
