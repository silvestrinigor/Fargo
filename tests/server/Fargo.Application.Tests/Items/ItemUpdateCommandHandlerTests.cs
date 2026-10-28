using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Users;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemUpdateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ItemUpdateCommandHandler CreateHandler()
        => new(
            new ItemService(itemRepository),
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            partitionRepository,
            auditLogRepository,
            currentActor);

    private static Item NewItem()
        => Item.CreateItem(Article.NewArticle(new Name("Widget")));

    [Fact]
    public async Task HandleAsync_WhenValidPlaceInContainer_ShouldUpdateAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditItem]);
        var item = NewItem();
        itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

        var parentContainer = Item.CreateItem(Article.NewArticleContainer(new Name("Box")));
        itemRepository.GetByGuidAsync(parentContainer.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(parentContainer));
        itemRepository.GetContainedDescendantGuidsAsync(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        var command = new ItemUpdateCommand(
            item.Guid, new ItemUpdateDto(ParentItemContainerGuid: parentContainer.Guid));

        await CreateHandler().HandleAsync(command);

        Assert.Equal(parentContainer.Guid, item.ParentItemContainerGuid);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemUpdateCommand(Guid.NewGuid(), new ItemUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemUpdateCommand(Guid.NewGuid(), new ItemUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenItemNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditItem]);
        var missing = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemUpdateCommand(missing, new ItemUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditItem], partitionAccesses: [Guid.NewGuid()]);
        var item = NewItem();
        itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemUpdateCommand(item.Guid, new ItemUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenParentContainerWouldCreateCircularHierarchy_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditItem]);
        var item = NewItem();
        itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(item));

        var parentContainer = Item.CreateItem(Article.NewArticleContainer(new Name("Box")));
        itemRepository.GetByGuidAsync(parentContainer.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(parentContainer));

        // Parent appears among the item's descendants -> circular assignment.
        itemRepository.GetContainedDescendantGuidsAsync(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([parentContainer.Guid]));

        var command = new ItemUpdateCommand(
            item.Guid, new ItemUpdateDto(ParentItemContainerGuid: parentContainer.Guid));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(command));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
