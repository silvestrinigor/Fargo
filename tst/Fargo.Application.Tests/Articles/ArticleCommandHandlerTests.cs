using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Application.Tests.Articles;

public sealed class ArticleCommandHandlerTests
{
    [Fact]
    public async Task Create_Should_CreateArticleWithOptionalStateAndSave()
    {
        var partition = Partition.CreatePartition(new Name("Partition"));
        var articleRepository = Substitute.For<IArticleRepository>();
        var partitionRepository = CreatePartitionRepository(partition);
        var entityEventRepository = Substitute.For<IEntityEventRepository>();
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var actor = CreateActor(
            [ActionType.CreateArticle, ActionType.EditArticle],
            [partition.Guid]);
        var handler = new ArticleCreateCommandHandler(
            articleRepository,
            partitionRepository,
            entityEventRepository,
            entityPartitionEventRepository,
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor),
            unitOfWork,
            Substitute.For<ILogger<ArticleCreateCommandHandler>>());

        var articleGuid = await handler.Handle(new ArticleCreateCommand(
            new ArticleCreateDto(
                new Name("Container"),
                Description: new Description("Description"),
                ShelfLife: TimeSpan.FromDays(7),
                Metrics: new ArticleMetricsDto(Mass.FromKilograms(2), Length.FromCentimeters(10)),
                Barcodes: new ArticleBarcodesDto(Ean13: new Ean13("1234567890123")),
                Partitions: [partition.Guid],
                IsActive: false,
                Container: new ArticleCreateContainerDto(Mass.FromKilograms(10)))));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.Guid == articleGuid &&
            article.IsContainer &&
            article.Description == new Description("Description") &&
            article.ShelfLife == TimeSpan.FromDays(7) &&
            article.Ean13 == new Ean13("1234567890123") &&
            article.Partitions.Single().Guid == partition.Guid &&
            !article.IsActive &&
            article.EditedByGuid == actor.ActorGuid &&
            article.ModificationTypes.HasFlag(ArticleModifiedType.General) &&
            article.ModificationTypes.HasFlag(ArticleModifiedType.Container) &&
            article.ModificationTypes.HasFlag(ArticleModifiedType.BarcodesChanged) &&
            article.ModificationTypes.HasFlag(ArticleModifiedType.PartitionsChanged)));
        entityEventRepository.Received(1).Add(Arg.Is<EntityEvent>(entityEvent =>
            entityEvent.EventType == EntityEventType.Created &&
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.EntityGuid == articleGuid &&
            entityEvent.ActorGuid == actor.ActorGuid));
        entityEventRepository.Received(1).Add(Arg.Is<EntityEvent>(entityEvent =>
            entityEvent.EventType == EntityEventType.Deactivated &&
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.EntityGuid == articleGuid &&
            entityEvent.ActorGuid == actor.ActorGuid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(partitionEvent =>
            partitionEvent.Event.EventType == EntityEventType.InsertedIntoPartition &&
            partitionEvent.EntityGuid == articleGuid &&
            partitionEvent.PartitionGuid == partition.Guid &&
            partitionEvent.ActorGuid == actor.ActorGuid));
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_Should_CreateVariationFromSourceArticle()
    {
        var sourceArticle = Article.CreateArticle(new Name("Source article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(sourceArticle);
        var handler = CreateCreateHandler(
            articleRepository,
            actor: CreateActor([ActionType.CreateArticle]));

        await handler.Handle(new ArticleCreateCommand(
            new ArticleCreateDto(
                new Name("Variation"),
                Variation: new ArticleCreateVariationDto(sourceArticle.Guid))));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsVariation &&
            article.Variation!.FromArticleGuid == sourceArticle.Guid &&
            article.ModificationTypes == (ArticleModifiedType.General | ArticleModifiedType.Relation)));
    }

    [Fact]
    public async Task Create_Should_CreatePackFromSourceArticle()
    {
        var sourceArticle = Article.CreateArticle(new Name("Source article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(sourceArticle);
        var handler = CreateCreateHandler(
            articleRepository,
            actor: CreateActor([ActionType.CreateArticle]));

        await handler.Handle(new ArticleCreateCommand(
            new ArticleCreateDto(
                new Name("Pack"),
                Pack: new ArticleCreatePackDto(sourceArticle.Guid, 4.Amount()))));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsPack &&
            article.Pack!.FromArticleGuid == sourceArticle.Guid &&
            article.Pack.Quantity.Equals(4.Amount(), 0.Amount())));
    }

    [Fact]
    public async Task Create_Should_CreateKitFromSourceArticles()
    {
        var firstArticle = Article.CreateArticle(new Name("First article"), CreateDomainActor());
        var secondArticle = Article.CreateArticle(new Name("Second article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(firstArticle, secondArticle);
        var handler = CreateCreateHandler(
            articleRepository,
            actor: CreateActor([ActionType.CreateArticle]));

        await handler.Handle(new ArticleCreateCommand(
            new ArticleCreateDto(
                new Name("Kit"),
                Kit: new ArticleCreateKitDto(
                [
                    new ArticleCreateKitPackDto(firstArticle.Guid, 2.Amount()),
                    new ArticleCreateKitPackDto(secondArticle.Guid, 3.Amount())
                ]))));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsKit &&
            article.Kit!.Components.Count == 2 &&
            article.ModificationTypes == (ArticleModifiedType.General | ArticleModifiedType.Relation)));
    }

    [Fact]
    public async Task Create_Should_Throw_WhenMoreThanOneSpecializedTypeIsProvided()
    {
        var handler = CreateCreateHandler();

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new ArticleCreateCommand(
                new ArticleCreateDto(
                    new Name("Invalid"),
                    Variation: new ArticleCreateVariationDto(Guid.NewGuid()),
                    Container: new ArticleCreateContainerDto()))));
    }

    [Fact]
    public async Task Create_Should_Throw_WhenKitHasNoPacks()
    {
        var handler = CreateCreateHandler();

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new ArticleCreateCommand(
                new ArticleCreateDto(
                    new Name("Invalid kit"),
                    Kit: new ArticleCreateKitDto([])))));
    }

    [Fact]
    public async Task Patch_Should_ApplySpecifiedFieldsAndSave()
    {
        var oldPartition = Partition.CreatePartition(new Name("Old partition"));
        var newPartition = Partition.CreatePartition(new Name("New partition"));
        var article = Article.CreateArticle(new Name("Article"), CreateDomainActor());
        article.AddPartition(oldPartition, CreateDomainActor());
        var articleRepository = CreateArticleRepository(article);
        var partitionRepository = CreatePartitionRepository(newPartition);
        var entityEventRepository = Substitute.For<IEntityEventRepository>();
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var actor = CreateActor(
            [ActionType.EditArticle],
            [oldPartition.Guid, newPartition.Guid]);
        var handler = new ArticlePatchCommandHandler(
            articleRepository,
            partitionRepository,
            entityEventRepository,
            entityPartitionEventRepository,
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor),
            unitOfWork,
            Substitute.For<ILogger<ArticlePatchCommandHandler>>());

        await handler.Handle(new ArticlePatchCommand(
            article.Guid,
            new ArticlePatchDto(
                Name: new Name("Renamed"),
                Description: new Description("Description"),
                Metrics: new ArticleMetricsDto(Mass.FromKilograms(1)),
                Barcodes: new ArticleBarcodesDto(Ean8: new Ean8("12345670")),
                Partitions: [newPartition.Guid],
                IsActive: false)));

        Assert.Equal("Renamed", article.Name.Value);
        Assert.Equal(new Description("Description"), article.Description);
        Assert.Equal(Mass.FromKilograms(1), article.Mass);
        Assert.Equal(new Ean8("12345670"), article.Ean8);
        Assert.Single(article.Partitions);
        Assert.Equal(newPartition.Guid, article.Partitions.Single().Guid);
        Assert.False(article.IsActive);
        Assert.Equal(actor.ActorGuid, article.EditedByGuid);
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.General));
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.MetricsChanged));
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.BarcodesChanged));
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.PartitionsChanged));
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.Deactivated));
        entityEventRepository.Received(1).Add(Arg.Is<EntityEvent>(entityEvent =>
            entityEvent.EventType == EntityEventType.Deactivated &&
            entityEvent.EntityGuid == article.Guid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(partitionEvent =>
            partitionEvent.Event.EventType == EntityEventType.InsertedIntoPartition &&
            partitionEvent.PartitionGuid == newPartition.Guid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(partitionEvent =>
            partitionEvent.Event.EventType == EntityEventType.RemovedFromPartition &&
            partitionEvent.PartitionGuid == oldPartition.Guid));
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Patch_Should_NotRecordActivationEvent_WhenAlreadyActive()
    {
        var article = Article.CreateArticle(new Name("Article"), CreateDomainActor());
        var entityEventRepository = Substitute.For<IEntityEventRepository>();
        var handler = new ArticlePatchCommandHandler(
            CreateArticleRepository(article),
            Substitute.For<IPartitionRepository>(),
            entityEventRepository,
            Substitute.For<IEntityPartitionEventRepository>(),
            new ArticleService(Substitute.For<IArticleRepository>()),
            CreateCurrentAuthorizationContext(CreateActor([ActionType.EditArticle])),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticlePatchCommandHandler>>());

        await handler.Handle(new ArticlePatchCommand(article.Guid, new ArticlePatchDto(IsActive: true)));

        entityEventRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task Delete_Should_RemoveArticleRecordDeletedEventAndSave()
    {
        var article = Article.CreateArticle(new Name("Article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(article);
        var entityEventRepository = Substitute.For<IEntityEventRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var actor = CreateActor([ActionType.DeleteArticle]);
        var handler = new ArticleDeleteCommandHandler(
            articleRepository,
            entityEventRepository,
            CreateCurrentAuthorizationContext(actor),
            unitOfWork,
            Substitute.For<ILogger<ArticleDeleteCommandHandler>>());

        await handler.Handle(new ArticleDeleteCommand(article.Guid));

        articleRepository.Received(1).Remove(article);
        entityEventRepository.Received(1).Add(Arg.Is<EntityEvent>(entityEvent =>
            entityEvent.EventType == EntityEventType.Deleted &&
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.EntityGuid == article.Guid &&
            entityEvent.ActorGuid == actor.ActorGuid));
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    private static ArticleCreateCommandHandler CreateCreateHandler(
        IArticleRepository? articleRepository = null,
        IPartitionRepository? partitionRepository = null,
        IEntityEventRepository? entityEventRepository = null,
        IEntityPartitionEventRepository? entityPartitionEventRepository = null,
        IAuthorizationContext? actor = null,
        IUnitOfWork? unitOfWork = null)
    {
        articleRepository ??= Substitute.For<IArticleRepository>();

        return new ArticleCreateCommandHandler(
            articleRepository,
            partitionRepository ?? Substitute.For<IPartitionRepository>(),
            entityEventRepository ?? Substitute.For<IEntityEventRepository>(),
            entityPartitionEventRepository ?? Substitute.For<IEntityPartitionEventRepository>(),
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor ?? CreateActor([ActionType.CreateArticle])),
            unitOfWork ?? Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticleCreateCommandHandler>>());
    }

    private static IArticleRepository CreateArticleRepository(params Article[] articles)
    {
        var repository = Substitute.For<IArticleRepository>();
        foreach (var article in articles)
        {
            SetIsEditStarted(article, false);
            repository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
                .Returns(article);
        }

        return repository;
    }

    private static IPartitionRepository CreatePartitionRepository(params Partition[] partitions)
    {
        var repository = Substitute.For<IPartitionRepository>();
        foreach (var partition in partitions)
        {
            repository.GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
                .Returns(partition);
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

    private static IAuthorizationContext CreateActor(
        IReadOnlyCollection<ActionType> permissions,
        IReadOnlyCollection<Guid>? partitionAccesses = null)
        => new AuthorizationContext(
            Guid.NewGuid(),
            IsAuthenticated: true,
            IsAdmin: false,
            permissions,
            partitionAccesses ?? [],
            UserGroupGuids: []);

    private static void SetIsEditStarted(Article article, bool value)
    {
        var property = typeof(Article).GetProperty(nameof(Article.IsEditStarted))!;

        property.SetValue(article, value);
    }
}
