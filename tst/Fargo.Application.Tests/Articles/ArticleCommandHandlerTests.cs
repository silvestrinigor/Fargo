using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.Articles;

public sealed class ArticleCommandHandlerTests
{
    [Fact]
    public async Task Create_Should_MarkActorAndGeneralModificationType()
    {
        var repository = Substitute.For<IArticleRepository>();
        var actor = CreateActor(ActionType.CreateArticle);
        var handler = new ArticleCreateCommandHandler(
            repository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleCreateCommandHandler>>());

        await handler.Handle(new ArticleCreateCommand(new Name("Article")));

        repository.Received(1).Add(Arg.Is<Article>(article =>
            article.EditedByGuid == actor.ActorGuid &&
            article.ModificationTypes == ArticleModifiedType.General));
    }

    [Fact]
    public async Task Rename_Should_MarkActorAndGeneralModificationType()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var repository = CreateArticleRepository(article);
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticleRenameCommandHandler(
            repository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleRenameCommandHandler>>());

        await handler.Handle(new ArticleRenameCommand(article.Guid, new Name("Renamed article")));

        Assert.Equal("Renamed article", article.Name.Value);
        Assert.Equal(actor.ActorGuid, article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.General, article.ModificationTypes);
    }

    [Fact]
    public async Task Rename_Should_NotMark_WhenNameIsUnchanged()
    {
        var previousActorGuid = Guid.NewGuid();
        var article = Article.CreateArticle(new Name("Article"));
        article.MarkAsEditedBy(previousActorGuid);
        article.MarkModificationType(ArticleModifiedType.BarcodesChanged);
        SetIsEditStarted(article, false);
        var repository = CreateArticleRepository(article);
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticleRenameCommandHandler(
            repository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleRenameCommandHandler>>());

        await handler.Handle(new ArticleRenameCommand(article.Guid, new Name("Article")));

        Assert.Equal(previousActorGuid, article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.BarcodesChanged, article.ModificationTypes);
    }

    [Fact]
    public async Task MultipleCommands_Should_AccumulateModificationTypesInSameSession()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var repository = CreateArticleRepository(article);
        var actor = CreateActor(ActionType.EditArticle);
        var currentAuthorizationContext = CreateCurrentAuthorizationContext(actor);
        var renameHandler = new ArticleRenameCommandHandler(
            repository,
            currentAuthorizationContext,
            Substitute.For<ILogger<ArticleRenameCommandHandler>>());
        var metricsHandler = new ArticleSetMetricsCommandHandler(
            repository,
            currentAuthorizationContext,
            Substitute.For<ILogger<ArticleSetMetricsCommandHandler>>());

        await renameHandler.Handle(new ArticleRenameCommand(article.Guid, new Name("Renamed article")));
        await metricsHandler.Handle(new ArticleSetMetricsCommand(
            article.Guid,
            new ArticleMetrics(mass: UnitsNet.Mass.FromKilograms(1))));

        Assert.Equal(actor.ActorGuid, article.EditedByGuid);
        Assert.Equal(
            ArticleModifiedType.General | ArticleModifiedType.MetricsChanged,
            article.ModificationTypes);
    }

    [Fact]
    public async Task SetBarcodes_Should_MarkActorAndBarcodeModificationType()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var repository = CreateArticleRepository(article);
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticleSetBarcodesCommandHandler(
            repository,
            new ArticleService(repository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleSetBarcodesCommandHandler>>());

        await handler.Handle(new ArticleSetBarcodesCommand(
            article.Guid,
            new ArticleBarcodesSet(Ean13: new Ean13("1234567890123"))));

        Assert.Equal("1234567890123", article.Ean13?.Code);
        Assert.Equal(actor.ActorGuid, article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.BarcodesChanged, article.ModificationTypes);
    }

    [Fact]
    public async Task SetBarcodes_Should_NotRequireSetArticleBarcode_WhenPayloadIsUnchanged()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var repository = CreateArticleRepository(article);
        await new ArticleService(repository).SetEan13(new Ean13("1234567890123"), article);
        var previousActorGuid = Guid.NewGuid();
        article.MarkAsEditedBy(previousActorGuid);
        article.MarkModificationType(ArticleModifiedType.General);
        SetIsEditStarted(article, false);
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticleSetBarcodesCommandHandler(
            repository,
            new ArticleService(repository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleSetBarcodesCommandHandler>>());

        await handler.Handle(new ArticleSetBarcodesCommand(
            article.Guid,
            new ArticleBarcodesSet(Ean13: new Ean13("1234567890123"))));

        Assert.Equal(previousActorGuid, article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.General, article.ModificationTypes);
    }

    private static IArticleRepository CreateArticleRepository(Article article)
    {
        var repository = Substitute.For<IArticleRepository>();
        repository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);

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

    private static void SetIsEditStarted(Article article, bool value)
    {
        var property = typeof(Article).GetProperty(nameof(Article.IsEditStarted))!;

        property.SetValue(article, value);
    }
}
