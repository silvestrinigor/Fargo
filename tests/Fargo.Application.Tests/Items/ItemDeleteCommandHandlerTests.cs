using Fargo.Application.Commands.Items;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Tests.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Events;
using Fargo.Core.Informations;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemDeleteCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IItemEventRepository itemEventRepository = Substitute.For<IItemEventRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ItemDeleteCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            itemEventRepository,
            currentActor);

    private static Item NewItem()
        => Item.CreateItem(Article.NewArticle(new Name("Widget")));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldRemoveItemAndRecordItsDeletion()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteItem]);
        var item = NewItem();
        itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

        await CreateHandler().HandleAsync(new ItemDeleteCommand(item.Guid));

        itemRepository.Received(1).Remove(item);
        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(
            e => e!.ItemGuid == item.Guid
                 && e.ItemEventType == ItemEventType.Deletion
                 && e.ToNoContainer));
    }

    [Fact]
    public async Task HandleAsync_WhenItemIsAContainer_ShouldRecordADeletionForEachContainedItem()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteItem]);
        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Box")));
        itemRepository.GetByGuidAsync(container.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(container));
        var inner = new ContainedItem(Guid.NewGuid(), Guid.NewGuid());
        itemRepository.GetContainedItemsAsync(container.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ContainedItem>>([inner]));

        await CreateHandler().HandleAsync(new ItemDeleteCommand(container.Guid));

        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(
            e => e!.ItemGuid == inner.ItemGuid
                 && e.ArticleGuid == inner.ArticleGuid
                 && e.ItemEventType == ItemEventType.Deletion
                 && e.FromItemContainerGuid == null));
        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(e => e!.ItemGuid == container.Guid));
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteItem]);
        var missing = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemDeleteCommand(missing)));
    }
}
