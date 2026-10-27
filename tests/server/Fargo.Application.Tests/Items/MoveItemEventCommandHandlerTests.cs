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
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitsNet;

namespace Fargo.Application.Tests.Items;

public class MoveItemEventCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IItemEventRepository itemEventRepository = Substitute.For<IItemEventRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private MoveItemEventCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            articleRepository,
            itemRepository,
            itemEventRepository,
            auditLogRepository,
            currentActor,
            unitOfWork,
            Substitute.For<ILogger<MoveItemEventCommandHandler>>());

    private static Scalar Qty(double amount) => Scalar.FromAmount(amount);

    [Fact]
    public async Task HandleAsync_WhenValidReceipt_ShouldAddEventAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Bulk sand"), ArticleTrackingMode.Event);
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Silo")));
        itemRepository.GetByGuidAsync(container.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(container));

        await CreateHandler().HandleAsync(new MoveItemEventCommand(
            new ItemMovementCreateDto(article.Guid, Qty(50), ToItemContainerGuid: container.Guid)));

        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(
            e => e.ArticleGuid == article.Guid
                 && e.EventType == ItemEventType.Moviment
                 && e.Moviment != null
                 && e.Moviment.Quantity.Amount == 50
                 && e.Moviment.ToItemContainerGuid == container.Guid));
        auditLogRepository.Received(1).Add(Arg.Is<AuditLog>(
            a => a.AuditLogType == AuditLogType.ItemEventRecorded
                 && a.EntityType == EntityType.ItemEvent));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenArticleIsEntityTracked_ShouldThrowAndNotPersist()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"), ArticleTrackingMode.Entity);
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));
        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Box")));
        itemRepository.GetByGuidAsync(container.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(container));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new MoveItemEventCommand(
                new ItemMovementCreateDto(article.Guid, Qty(1), ToItemContainerGuid: container.Guid))));

        itemEventRepository.DidNotReceive().Add(Arg.Any<ItemEvent>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new MoveItemEventCommand(
                new ItemMovementCreateDto(Guid.NewGuid(), Qty(1), ToItemContainerGuid: Guid.NewGuid()))));
    }

    [Fact]
    public async Task HandleAsync_WhenArticleNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var missing = Guid.NewGuid();
        articleRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new MoveItemEventCommand(
                new ItemMovementCreateDto(missing, Qty(1), ToItemContainerGuid: Guid.NewGuid()))));
    }

    [Fact]
    public async Task HandleAsync_WhenContainerNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Bulk sand"), ArticleTrackingMode.Event);
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));
        var missingContainer = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missingContainer, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new MoveItemEventCommand(
                new ItemMovementCreateDto(article.Guid, Qty(1), ToItemContainerGuid: missingContainer))));
    }
}
