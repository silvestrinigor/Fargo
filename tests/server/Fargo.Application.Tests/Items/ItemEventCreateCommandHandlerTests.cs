using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Core.Items.Events;
using Fargo.Core.Users;
using NSubstitute;
using UnitsNet;

namespace Fargo.Application.Tests.Items;

public class ItemEventCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IItemEventRepository itemEventRepository = Substitute.For<IItemEventRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ItemEventCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            articleRepository,
            itemRepository,
            itemEventRepository,
            auditLogRepository,
            currentActor);

    private static Scalar Qty(double amount) => Scalar.FromAmount(amount);

    private Article ConfigureEventArticle()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Bulk sand"), ArticleTrackingMode.Event);
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));
        return article;
    }

    private Item ConfigureContainer()
    {
        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Silo")));
        itemRepository.GetByGuidAsync(container.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(container));
        return container;
    }

    [Fact]
    public async Task HandleAsync_WhenMovementReceipt_ShouldRecordMovimentAndReturnGuid()
    {
        var article = ConfigureEventArticle();
        var container = ConfigureContainer();

        var result = await CreateHandler().HandleAsync(new ItemEventCreateCommand(
            new ItemEventCreateDto(ItemEventType.Moviment, article.Guid, Qty(50), ToItemContainerGuid: container.Guid)));

        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(
            e => e.Guid == result
                 && e.EventType == ItemEventType.Moviment
                 && e.Moviment != null
                 && e.Moviment.Quantity.Amount == 50
                 && e.Moviment.ToItemContainerGuid == container.Guid));
        auditLogRepository.Received(1).Add(Arg.Is<AuditLog>(
            a => a.AuditLogType == AuditLogType.ItemEventRecorded && a.EntityType == EntityType.ItemEvent));
    }

    [Fact]
    public async Task HandleAsync_WhenCreation_ShouldRecordLifecycleAndReturnGuid()
    {
        var article = ConfigureEventArticle();
        var container = ConfigureContainer();

        var result = await CreateHandler().HandleAsync(new ItemEventCreateCommand(
            new ItemEventCreateDto(ItemEventType.Creation, article.Guid, Qty(120), ItemContainerGuid: container.Guid)));

        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(
            e => e.Guid == result
                 && e.EventType == ItemEventType.Creation
                 && e.Lifecycle != null
                 && e.Lifecycle.Quantity.Amount == 120
                 && e.Lifecycle.ItemContainerGuid == container.Guid
                 && e.Moviment == null));
    }

    [Fact]
    public async Task HandleAsync_WhenDeletion_ShouldRecordLifecycleAndReturnGuid()
    {
        var article = ConfigureEventArticle();

        var result = await CreateHandler().HandleAsync(new ItemEventCreateCommand(
            new ItemEventCreateDto(ItemEventType.Deletion, article.Guid, Qty(7))));

        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(
            e => e.Guid == result
                 && e.EventType == ItemEventType.Deletion
                 && e.Lifecycle != null
                 && e.Lifecycle.Quantity.Amount == 7
                 && e.Lifecycle.NoContainer));
    }

    [Fact]
    public async Task HandleAsync_WhenArticleIsEntityTracked_ShouldThrowAndNotPersist()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"), ArticleTrackingMode.Entity);
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new ItemEventCreateCommand(
                new ItemEventCreateDto(ItemEventType.Creation, article.Guid, Qty(1)))));

        itemEventRepository.DidNotReceive().Add(Arg.Any<ItemEvent>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemEventCreateCommand(
                new ItemEventCreateDto(ItemEventType.Creation, Guid.NewGuid(), Qty(1)))));
    }

    [Fact]
    public async Task HandleAsync_WhenArticleNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var missing = Guid.NewGuid();
        articleRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemEventCreateCommand(
                new ItemEventCreateDto(ItemEventType.Creation, missing, Qty(1)))));
    }

    [Fact]
    public async Task HandleAsync_WhenContainerNotFound_ShouldThrow()
    {
        var article = ConfigureEventArticle();
        var missingContainer = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missingContainer, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemEventCreateCommand(
                new ItemEventCreateDto(ItemEventType.Creation, article.Guid, Qty(1), ItemContainerGuid: missingContainer))));
    }
}
