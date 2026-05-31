using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Shared.Items;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public sealed class ItemMovementCommandHandlerTests
{
    [Fact]
    public async Task ItemUpdate_Should_RecordMovement_WhenParentChanges()
    {
        var item = CreateItem();
        var parent = CreateContainerItem();
        var itemRepository = CreateItemRepository(item, parent);
        itemRepository
            .GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);
        var movementRepository = Substitute.For<IItemMovementRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IEntityEventRepository>(),
            Substitute.For<IEntityPartitionEventRepository>(),
            movementRepository,
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.Handle(new ItemUpdateCommand(item.Guid, new ItemUpdateDto([], parent.Guid)));

        Assert.Equal(actor.ActorGuid, item.EditedByGuid);
        Assert.Equal(ItemModifiedType.ParentContainerChanged, item.ModificationTypes);
        movementRepository.Received(1).Add(Arg.Is<ItemMovement>(movement =>
            movement.Event.EntityType == EntityType.Item &&
            movement.Event.EventType == EventType.Moved &&
            movement.ItemGuid == item.Guid &&
            movement.FromParentContainerGuid == null &&
            movement.ToParentContainerGuid == parent.Guid &&
            movement.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task ItemUpdate_Should_NotRecordMovement_WhenParentIsUnchanged()
    {
        var item = CreateItem();
        var parent = CreateContainerItem();
        var itemRepository = CreateItemRepository(item, parent);
        itemRepository
            .GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);
        await new ItemService(itemRepository).MoveToContainer(parent, item, CreateDomainActor());
        var originalEditedByGuid = item.EditedByGuid;
        var originalModificationTypes = item.ModificationTypes;
        var movementRepository = Substitute.For<IItemMovementRepository>();
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IEntityEventRepository>(),
            Substitute.For<IEntityPartitionEventRepository>(),
            movementRepository,
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditItem)),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.Handle(new ItemUpdateCommand(item.Guid, new ItemUpdateDto([], parent.Guid)));

        Assert.Equal(originalEditedByGuid, item.EditedByGuid);
        Assert.Equal(originalModificationTypes, item.ModificationTypes);
        movementRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task ItemUpdate_Should_RecordMovement_WhenItemIsRemovedFromContainer()
    {
        var item = CreateItem();
        var parent = CreateContainerItem();
        var itemRepository = CreateItemRepository(item, parent);
        itemRepository
            .GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);
        await new ItemService(itemRepository).MoveToContainer(parent, item, CreateDomainActor());
        var movementRepository = Substitute.For<IItemMovementRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IEntityEventRepository>(),
            Substitute.For<IEntityPartitionEventRepository>(),
            movementRepository,
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.Handle(new ItemUpdateCommand(item.Guid, new ItemUpdateDto([], null)));

        Assert.Equal(actor.ActorGuid, item.EditedByGuid);
        Assert.Equal(ItemModifiedType.ParentContainerChanged, item.ModificationTypes);
        movementRepository.Received(1).Add(Arg.Is<ItemMovement>(movement =>
            movement.Event.EntityType == EntityType.Item &&
            movement.Event.EventType == EventType.Moved &&
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
        => Item.CreateItem(Article.CreateArticle(new Name("Article"), CreateDomainActor()));

    private static Item CreateContainerItem()
        => Item.CreateItem(Article.CreateArticleContainer(new Name("Container article"), CreateDomainActor()));
}
