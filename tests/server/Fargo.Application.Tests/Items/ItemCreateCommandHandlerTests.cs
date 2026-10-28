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

public class ItemCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ItemCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            articleRepository,
            partitionRepository,
            auditLogRepository,
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
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
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
}
