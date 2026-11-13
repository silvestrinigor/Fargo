using Fargo.Application.Commands.Items;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.Items;
using Fargo.Application.Persistence;
using Fargo.Application.Tests.Common;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using NSubstitute;
using UnitsNet;

namespace Fargo.Application.Tests.Items;

public class ItemCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ItemCreateCommandHandler CreateHandler()
        => new(
            new ItemService(itemRepository),
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            articleRepository,
            partitionRepository,
            currentActor);

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddItemAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        var result = await CreateHandler().HandleAsync(
            new ItemCreateCommand(new ItemCreateDto(article.Guid)));

        itemRepository.Received(1).Add(Arg.Is<Item>(i => i.Guid == result && i.ArticleGuid == article.Guid));
    }

    [Fact]
    public async Task HandleAsync_WhenValidWithParentContainer_ShouldPlaceInsideContainer()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        var containerArticle = Article.NewArticleContainer(new Name("Box"));
        var parentContainer = Item.CreateItem(containerArticle);
        itemRepository.GetByGuidAsync(parentContainer.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(parentContainer));

        await CreateHandler().HandleAsync(
            new ItemCreateCommand(new ItemCreateDto(article.Guid, ParentItemContainerGuid: parentContainer.Guid)));

        itemRepository.Received(1).Add(Arg.Is<Item>(i => i.ParentItemContainerGuid == parentContainer.Guid));
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemCreateCommand(new ItemCreateDto(Guid.NewGuid()))));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemCreateCommand(new ItemCreateDto(Guid.NewGuid()))));
    }

    [Fact]
    public async Task HandleAsync_WhenArticleNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var missing = Guid.NewGuid();
        articleRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemCreateCommand(new ItemCreateDto(missing))));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToArticle_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.CreateItem], partitionAccesses: [Guid.NewGuid()]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemCreateCommand(new ItemCreateDto(article.Guid))));
    }

    [Fact]
    public async Task HandleAsync_WhenParentContainerIsNotAContainer_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        // Parent item is backed by a non-container article -> PlaceInsideContainer throws.
        var nonContainerParent = Item.CreateItem(Article.NewArticle(new Name("Not a box")));
        itemRepository.GetByGuidAsync(nonContainerParent.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(nonContainerParent));

        var command = new ItemCreateCommand(
            new ItemCreateDto(article.Guid, ParentItemContainerGuid: nonContainerParent.Guid));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(command));

        itemRepository.DidNotReceive().Add(Arg.Any<Item>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenParentContainerNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));
        var missingParent = Guid.NewGuid();
        itemRepository.GetByGuidAsync(missingParent, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(null));

        var command = new ItemCreateCommand(
            new ItemCreateDto(article.Guid, ParentItemContainerGuid: missingParent));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenArticleNotInContainerAllowList_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        // The container only allows a DIFFERENT article, so placing this item must fail.
        var otherAllowed = Article.NewArticle(new Name("Allowed"));
        var containerArticle = Article.NewArticleContainer(
            new Name("Box"), allowedArticles: [(otherAllowed, null)]);
        var parentContainer = Item.CreateItem(containerArticle);
        itemRepository.GetByGuidAsync(parentContainer.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(parentContainer));

        var command = new ItemCreateCommand(
            new ItemCreateDto(article.Guid, ParentItemContainerGuid: parentContainer.Guid));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(command));

        itemRepository.DidNotReceive().Add(Arg.Any<Item>());
    }

    [Fact]
    public async Task HandleAsync_WhenArticleAllowedButMaxQuantityReached_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        var containerArticle = Article.NewArticleContainer(
            new Name("Box"), allowedArticles: [(article, Scalar.FromAmount(2))]);
        var parentContainer = Item.CreateItem(containerArticle);
        itemRepository.GetByGuidAsync(parentContainer.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(parentContainer));

        // The container already holds the maximum (2) of this article.
        itemRepository.CountDirectChildrenByArticleAsync(
            parentContainer.Guid, article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(2));

        var command = new ItemCreateCommand(
            new ItemCreateDto(article.Guid, ParentItemContainerGuid: parentContainer.Guid));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(command));

        itemRepository.DidNotReceive().Add(Arg.Any<Item>());
    }

    [Fact]
    public async Task HandleAsync_WhenArticleAllowedAndUnderMaxQuantity_ShouldPlaceInsideContainer()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateItem]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        var containerArticle = Article.NewArticleContainer(
            new Name("Box"), allowedArticles: [(article, Scalar.FromAmount(2))]);
        var parentContainer = Item.CreateItem(containerArticle);
        itemRepository.GetByGuidAsync(parentContainer.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(parentContainer));

        // One already inside; adding a second stays within the cap of 2.
        itemRepository.CountDirectChildrenByArticleAsync(
            parentContainer.Guid, article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        await CreateHandler().HandleAsync(
            new ItemCreateCommand(new ItemCreateDto(article.Guid, ParentItemContainerGuid: parentContainer.Guid)));

        itemRepository.Received(1).Add(Arg.Is<Item>(i => i.ParentItemContainerGuid == parentContainer.Guid));
    }
}
