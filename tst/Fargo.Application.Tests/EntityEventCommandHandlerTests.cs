using Fargo.Application.Articles.Commands;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Shared.Articles;
using Fargo.Application.Shared.Items;
using Fargo.Core.Activables;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests;

public sealed class EntityEventCommandHandlerTests
{
    [Fact]
    public async Task ArticleCreate_Should_RecordCreatedEvent()
    {
        var articleRepository = Substitute.For<IArticleRepository>();
        var entityEventRepository = Substitute.For<IEventRepository>();
        var actor = CreateActor(ActionType.CreateArticle);
        var handler = new ArticleCreateDefaultCommandHandler(
            articleRepository,
            Substitute.For<IPartitionRepository>(),
            entityEventRepository,
            Substitute.For<IPartitionEventRepository>(),
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticleCreateDefaultCommandHandler>>());

        var articleGuid = await handler.Handle(new ArticleCreateDefaultCommand(
            new Name("Article")));

        entityEventRepository.Received(1).Add(Arg.Is<Event>(entityEvent =>
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.EventType == EventType.Created &&
            entityEvent.EntityGuid == articleGuid &&
            entityEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task ArticleDeactivate_Should_RecordDeactivatedEvent_WhenStateChanges()
    {
        var article = Article.NewArticle(new Name("Article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(article);
        var entityEventRepository = Substitute.For<IEventRepository>();
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticlePatchCommandHandler(
            articleRepository,
            Substitute.For<IPartitionRepository>(),
            entityEventRepository,
            Substitute.For<IPartitionEventRepository>(),
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticlePatchCommandHandler>>());

        await handler.Handle(new ArticlePatchCommand(article.Guid, new ArticlePatchDto(IsActive: false)));

        entityEventRepository.Received(1).Add(Arg.Is<Event>(entityEvent =>
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.EventType == EventType.Deactivated &&
            entityEvent.EntityGuid == article.Guid &&
            entityEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task ArticleActivate_Should_NotRecordEvent_WhenAlreadyActive()
    {
        var article = Article.NewArticle(new Name("Article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(article);
        var entityEventRepository = Substitute.For<IEventRepository>();
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticlePatchCommandHandler(
            articleRepository,
            Substitute.For<IPartitionRepository>(),
            entityEventRepository,
            Substitute.For<IPartitionEventRepository>(),
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticlePatchCommandHandler>>());

        await handler.Handle(new ArticlePatchCommand(article.Guid, new ArticlePatchDto(IsActive: true)));

        entityEventRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task ItemDelete_Should_RecordDeletedEvent()
    {
        var item = Item.CreateItem(Article.NewArticle(new Name("Article"), CreateDomainActor()));
        var itemRepository = CreateItemRepository(item);
        var entityEventRepository = Substitute.For<IEventRepository>();
        var actor = CreateActor(ActionType.DeleteItem);
        var handler = new ItemDeleteCommandHandler(
            itemRepository,
            entityEventRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemDeleteCommandHandler>>());

        await handler.Handle(new ItemDeleteCommand(item.Guid));

        entityEventRepository.Received(1).Add(Arg.Is<Event>(entityEvent =>
            entityEvent.EntityType == EntityType.Item &&
            entityEvent.EventType == EventType.Deleted &&
            entityEvent.EntityGuid == item.Guid &&
            entityEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task ItemCreate_Should_Throw_WhenArticleIsInactive()
    {
        var article = Article.NewArticle(new Name("Article"), CreateDomainActor());
        article.Deactivate(CreateDomainActor());
        var itemRepository = Substitute.For<IItemRepository>();
        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);
        var handler = new ItemCreateCommandHandler(
            itemRepository,
            articleRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IEventRepository>(),
            Substitute.For<IPartitionEventRepository>(),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.CreateItem)),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemCreateCommandHandler>>());

        await Assert.ThrowsAsync<EntityNotActiveException<Article>>(
            () => handler.Handle(new ItemCreateCommand(new ItemCreateDto(article.Guid))));

        itemRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task ItemDeactivate_Should_RecordDeactivatedEvent_WhenStateChanges()
    {
        var item = Item.CreateItem(Article.NewArticle(new Name("Article"), CreateDomainActor()));
        var itemRepository = CreateItemRepository(item);
        var entityEventRepository = Substitute.For<IEventRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            Substitute.For<IPartitionRepository>(),
            entityEventRepository,
            Substitute.For<IPartitionEventRepository>(),
            Substitute.For<IItemMovementRepository>(),
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.Handle(new ItemUpdateCommand(item.Guid, new ItemUpdateDto([], IsActive: false)));

        Assert.False(item.IsActive);
        Assert.Equal(actor.ActorGuid, item.EditedByActorGuid);
        Assert.Equal(ItemModifiedType.Deactivated, item.ModificationTypes);
        entityEventRepository.Received(1).Add(Arg.Is<Event>(entityEvent =>
            entityEvent.EntityType == EntityType.Item &&
            entityEvent.EventType == EventType.Deactivated &&
            entityEvent.EntityGuid == item.Guid &&
            entityEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task ItemActivate_Should_RecordActivatedEvent_WhenStateChanges()
    {
        var item = Item.CreateItem(Article.NewArticle(new Name("Article"), CreateDomainActor()));
        item.Deactivate();
        var itemRepository = CreateItemRepository(item);
        var entityEventRepository = Substitute.For<IEventRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            Substitute.For<IPartitionRepository>(),
            entityEventRepository,
            Substitute.For<IPartitionEventRepository>(),
            Substitute.For<IItemMovementRepository>(),
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.Handle(new ItemUpdateCommand(item.Guid, new ItemUpdateDto([], IsActive: true)));

        Assert.True(item.IsActive);
        Assert.Equal(actor.ActorGuid, item.EditedByActorGuid);
        Assert.Equal(ItemModifiedType.Activated, item.ModificationTypes);
        entityEventRepository.Received(1).Add(Arg.Is<Event>(entityEvent =>
            entityEvent.EntityType == EntityType.Item &&
            entityEvent.EventType == EventType.Activated &&
            entityEvent.EntityGuid == item.Guid &&
            entityEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task ItemActivate_Should_NotRecordEvent_WhenAlreadyActive()
    {
        var item = Item.CreateItem(Article.NewArticle(new Name("Article"), CreateDomainActor()));
        var itemRepository = CreateItemRepository(item);
        var entityEventRepository = Substitute.For<IEventRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            Substitute.For<IPartitionRepository>(),
            entityEventRepository,
            Substitute.For<IPartitionEventRepository>(),
            Substitute.For<IItemMovementRepository>(),
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.Handle(new ItemUpdateCommand(item.Guid, new ItemUpdateDto([], IsActive: true)));

        Assert.Null(item.EditedByActorGuid);
        Assert.Equal(ItemModifiedType.None, item.ModificationTypes);
        entityEventRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    private static IArticleRepository CreateArticleRepository(Article article)
    {
        var repository = Substitute.For<IArticleRepository>();
        repository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);

        return repository;
    }

    private static IItemRepository CreateItemRepository(Item item)
    {
        var repository = Substitute.For<IItemRepository>();
        repository.GetByGuid(item.Guid, Arg.Any<CancellationToken>())
            .Returns(item);

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
}
