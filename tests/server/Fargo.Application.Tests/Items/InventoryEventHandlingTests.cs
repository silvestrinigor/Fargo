using Fargo.Application.Articles;
using Fargo.Application.Items;
using Fargo.Application.Tests.Common;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Users;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class InventoryEventHandlingTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly TestCurrentActor currentActor = new();

    // --- Container inventory: entity + event merge -------------------------

    [Fact]
    public async Task ContainerInventory_ShouldMergeEntityAndEventCounts()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid);

        var itemQueryRepository = Substitute.For<IItemQueryRepository>();
        var itemRepository = Substitute.For<IItemRepository>();

        var container = Guid.NewGuid();
        var entityArticle = Guid.NewGuid();
        var eventArticle = Guid.NewGuid();

        itemQueryRepository.ExistByGuidAsync(container, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        itemRepository.GetContainedDescendantGuidsAsync(container, true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([container]));

        itemQueryRepository.GetInventoryInfoByGuidAsync(
                Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ItemContainerInventoryDto>>(
                [new ItemContainerInventoryDto(entityArticle, 3)]));

        itemQueryRepository.GetEventInventoryInfoByGuidAsync(
                container, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<bool>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ItemContainerInventoryDto>>(
                [new ItemContainerInventoryDto(eventArticle, 12.5)]));

        var handler = new ItemContainerInventoryByGuidQueryHandler(
            ActorTestFactory.Resolver(userRepository),
            itemQueryRepository,
            itemRepository,
            currentActor);

        var result = await handler.HandleAsync(new ItemContainerInventoryByGuidQuery(container));

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Contains(result, d => d.ArticleGuid == entityArticle && d.TotalCount == 3);
        Assert.Contains(result, d => d.ArticleGuid == eventArticle && d.TotalCount == 12.5);
    }

    // --- Article inventory: branch on tracking mode ------------------------

    [Fact]
    public async Task ArticleInventory_WhenEventTracked_ShouldUseEventQuery()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid);

        var articleRepository = Substitute.For<IArticleQueryRepository>();
        var article = Guid.NewGuid();

        articleRepository.GetTrackingModeByGuidAsync(article, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ArticleTrackingMode?>(ArticleTrackingMode.Event));

        articleRepository.GetEventInventoryInfoByGuidAsync(
                article, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<bool>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ArticleInventoryDto(42.0)));

        var handler = new ArticleInventoryByGuidQueryHandler(
            ActorTestFactory.Resolver(userRepository),
            articleRepository,
            currentActor);

        var result = await handler.HandleAsync(new ArticleInventoryByGuidQuery(article));

        Assert.NotNull(result);
        Assert.Equal(42.0, result!.TotalCount);
        await articleRepository.Received(1).GetEventInventoryInfoByGuidAsync(
            article, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<bool>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArticleInventory_WhenEntityTracked_ShouldUseItemQuery()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid);

        var articleRepository = Substitute.For<IArticleQueryRepository>();
        var article = Guid.NewGuid();

        articleRepository.GetTrackingModeByGuidAsync(article, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ArticleTrackingMode?>(ArticleTrackingMode.Entity));

        articleRepository.GetInventoryInfoByGuidAsync(
                article, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ArticleInventoryDto(7.0)));

        var handler = new ArticleInventoryByGuidQueryHandler(
            ActorTestFactory.Resolver(userRepository),
            articleRepository,
            currentActor);

        var result = await handler.HandleAsync(new ArticleInventoryByGuidQuery(article));

        Assert.NotNull(result);
        Assert.Equal(7.0, result!.TotalCount);
        await articleRepository.DidNotReceive().GetEventInventoryInfoByGuidAsync(
            Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<bool>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArticleInventory_WhenArticleMissing_ShouldReturnNull()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid);

        var articleRepository = Substitute.For<IArticleQueryRepository>();
        var article = Guid.NewGuid();

        articleRepository.GetTrackingModeByGuidAsync(article, Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ArticleTrackingMode?>(null));

        var handler = new ArticleInventoryByGuidQueryHandler(
            ActorTestFactory.Resolver(userRepository),
            articleRepository,
            currentActor);

        var result = await handler.HandleAsync(new ArticleInventoryByGuidQuery(article));

        Assert.Null(result);
    }
}
