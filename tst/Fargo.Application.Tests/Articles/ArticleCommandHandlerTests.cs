using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitsNet.NumberExtensions.NumberToScalar;

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
            Substitute.For<IEntityEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleCreateCommandHandler>>());

        await handler.Handle(new ArticleCreateCommand(new Name("Article")));

        repository.Received(1).Add(Arg.Is<Article>(article =>
            article.EditedByGuid == actor.ActorGuid &&
            article.ModificationTypes == ArticleModifiedType.General));
    }

    [Fact]
    public async Task CreateVariation_Should_CreateVariationAndMarkRelationModificationType()
    {
        var fromArticle = Article.CreateArticle(new Name("Source article"));
        var repository = CreateArticleRepository(fromArticle);
        var entityEventRepository = Substitute.For<IEntityEventRepository>();
        var actor = CreateActor(ActionType.CreateArticle);
        var handler = new ArticleCreateVariationCommandHandler(
            repository,
            entityEventRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleCreateVariationCommandHandler>>());

        await handler.Handle(new ArticleCreateVariationCommand(new Name("Variation article"), fromArticle.Guid));

        repository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsVariation &&
            article.Variation!.FromArticleGuid == fromArticle.Guid &&
            article.EditedByGuid == actor.ActorGuid &&
            article.ModificationTypes == (ArticleModifiedType.General | ArticleModifiedType.Relation)));
        entityEventRepository.Received(1).Add(Arg.Is<EntityEvent>(entityEvent =>
            entityEvent.EventType == EntityEventType.Created &&
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task CreatePack_Should_CreatePackAndMarkRelationModificationType()
    {
        var fromArticle = Article.CreateArticle(new Name("Source article"));
        var repository = CreateArticleRepository(fromArticle);
        var actor = CreateActor(ActionType.CreateArticle);
        var handler = new ArticleCreatePackCommandHandler(
            repository,
            Substitute.For<IEntityEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleCreatePackCommandHandler>>());

        await handler.Handle(new ArticleCreatePackCommand(new Name("Pack article"), fromArticle.Guid, 4.Amount()));

        repository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsPack &&
            article.Pack!.FromArticleGuid == fromArticle.Guid &&
            article.Pack.Quantity.Equals(4.Amount(), 0.Amount()) &&
            article.ModificationTypes == (ArticleModifiedType.General | ArticleModifiedType.Relation)));
    }

    [Fact]
    public async Task CreateKit_Should_CreateKitAndMarkRelationModificationType()
    {
        var firstArticle = Article.CreateArticle(new Name("First source article"));
        var secondArticle = Article.CreateArticle(new Name("Second source article"));
        var repository = CreateArticleRepository(firstArticle, secondArticle);
        var actor = CreateActor(ActionType.CreateArticle);
        var handler = new ArticleCreateKitCommandHandler(
            repository,
            Substitute.For<IEntityEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleCreateKitCommandHandler>>());

        await handler.Handle(new ArticleCreateKitCommand(
            new Name("Kit article"),
            [
                new ArticleKitComponent(firstArticle.Guid, 2.Amount()),
                new ArticleKitComponent(secondArticle.Guid, 3.Amount()),
            ]));

        repository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsKit &&
            article.Kit!.FromArticles.Count == 2 &&
            article.ModificationTypes == (ArticleModifiedType.General | ArticleModifiedType.Relation)));
    }

    [Fact]
    public async Task CreateContainer_Should_CreateContainerAndMarkContainerModificationType()
    {
        var repository = Substitute.For<IArticleRepository>();
        var actor = CreateActor(ActionType.CreateArticle);
        var handler = new ArticleCreateContainerCommandHandler(
            repository,
            Substitute.For<IEntityEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleCreateContainerCommandHandler>>());

        await handler.Handle(new ArticleCreateContainerCommand(new Name("Container article")));

        repository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsContainer &&
            !article.Container!.MaxMass.HasValue &&
            article.ModificationTypes == (ArticleModifiedType.General | ArticleModifiedType.Container)));
    }

    [Fact]
    public async Task SetContainerMaxMass_Should_SetMaxMassAndMarkContainerModificationType()
    {
        var article = Article.CreateArticleContainer(new Name("Container article"));
        var repository = CreateArticleRepository(article);
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticleSetContainerMaxMassCommandHandler(
            repository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleSetContainerMaxMassCommandHandler>>());

        await handler.Handle(new ArticleSetContainerMaxMassCommand(
            article.Guid,
            UnitsNet.Mass.FromKilograms(10)));

        Assert.True(article.Container!.MaxMass!.Value.Equals(
            UnitsNet.Mass.FromKilograms(10),
            UnitsNet.Mass.Zero));
        Assert.Equal(actor.ActorGuid, article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.Container, article.ModificationTypes);
    }

    [Fact]
    public async Task SetContainerMaxMass_Should_NotMark_WhenMaxMassIsUnchanged()
    {
        var article = Article.CreateArticleContainer(new Name("Container article"));
        article.SetContainerMaxMass(UnitsNet.Mass.FromKilograms(10));
        var previousActorGuid = Guid.NewGuid();
        article.MarkAsEditedBy(previousActorGuid);
        article.MarkModificationType(ArticleModifiedType.General);
        SetIsEditStarted(article, false);
        var repository = CreateArticleRepository(article);
        var handler = new ArticleSetContainerMaxMassCommandHandler(
            repository,
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditArticle)),
            Substitute.For<ILogger<ArticleSetContainerMaxMassCommandHandler>>());

        await handler.Handle(new ArticleSetContainerMaxMassCommand(
            article.Guid,
            UnitsNet.Mass.FromKilograms(10)));

        Assert.Equal(previousActorGuid, article.EditedByGuid);
        Assert.Equal(ArticleModifiedType.General, article.ModificationTypes);
    }

    [Fact]
    public async Task SetContainerMaxMass_Should_Throw_WhenArticleIsNotContainer()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var repository = CreateArticleRepository(article);
        var handler = new ArticleSetContainerMaxMassCommandHandler(
            repository,
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditArticle)),
            Substitute.For<ILogger<ArticleSetContainerMaxMassCommandHandler>>());

        await Assert.ThrowsAsync<ArticleIsNotContainerFargoDomainException>(
            () => handler.Handle(new ArticleSetContainerMaxMassCommand(
                article.Guid,
                UnitsNet.Mass.FromKilograms(10))));
    }

    [Fact]
    public async Task CreateVariation_Should_Throw_WhenSourceArticleIsInactive()
    {
        var fromArticle = Article.CreateArticle(new Name("Source article"));
        fromArticle.Deactivate();
        var repository = CreateArticleRepository(fromArticle);
        var handler = new ArticleCreateVariationCommandHandler(
            repository,
            Substitute.For<IEntityEventRepository>(),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.CreateArticle)),
            Substitute.For<ILogger<ArticleCreateVariationCommandHandler>>());

        await Assert.ThrowsAsync<EntityNotActiveFargoDomainException<Article>>(
            () => handler.Handle(new ArticleCreateVariationCommand(new Name("Variation article"), fromArticle.Guid)));
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

    private static IArticleRepository CreateArticleRepository(params Article[] articles)
    {
        var repository = Substitute.For<IArticleRepository>();
        foreach (var article in articles)
        {
            repository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
                .Returns(article);
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

    private static void SetIsEditStarted(Article article, bool value)
    {
        var property = typeof(Article).GetProperty(nameof(Article.IsEditStarted))!;

        property.SetValue(article, value);
    }
}
