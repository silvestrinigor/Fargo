using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
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
        var handler = new ArticleCreateContainerCommandHandler(
            articleRepository,
            partitionRepository,
            entityEventRepository,
            entityPartitionEventRepository,
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor),
            unitOfWork,
            Substitute.For<ILogger<ArticleCreateContainerCommandHandler>>());

        var articleGuid = await handler.Handle(new ArticleCreateContainerCommand(
            new Name("Container"),
            MaxMass: Mass.FromKilograms(10),
            Description: new Description("Description"),
            ShelfLife: TimeSpan.FromDays(7),
            Metrics: new ArticleMetricsDto(Mass.FromKilograms(2), Length.FromCentimeters(10)),
            Barcodes: new ArticleBarcodesDto(Ean13: new Ean13("1234567890123")),
            Partitions: [partition.Guid],
            IsActive: false));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.Guid == articleGuid &&
            article.IsContainer &&
            article.ArticleType == ArticleType.Container &&
            article.Description == new Description("Description") &&
            article.ShelfLife == TimeSpan.FromDays(7) &&
            article.Ean13 == new Ean13("1234567890123") &&
            article.Partitions.Single().Guid == partition.Guid &&
            !article.IsActive &&
            article.EditedByActorGuid == actor.ActorGuid &&
            article.ModificationTypes.HasFlag(ArticleModifiedType.General) &&
            article.ModificationTypes.HasFlag(ArticleModifiedType.Container) &&
            article.ModificationTypes.HasFlag(ArticleModifiedType.BarcodesChanged) &&
            article.ModificationTypes.HasFlag(ArticleModifiedType.PartitionsChanged)));
        entityEventRepository.Received(1).Add(Arg.Is<EntityEvent>(entityEvent =>
            entityEvent.EventType == EventType.Created &&
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.EntityGuid == articleGuid &&
            entityEvent.ActorGuid == actor.ActorGuid));
        entityEventRepository.Received(1).Add(Arg.Is<EntityEvent>(entityEvent =>
            entityEvent.EventType == EventType.Deactivated &&
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.EntityGuid == articleGuid &&
            entityEvent.ActorGuid == actor.ActorGuid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(partitionEvent =>
            partitionEvent.Event.EventType == EventType.InsertedIntoPartition &&
            partitionEvent.EntityGuid == articleGuid &&
            partitionEvent.PartitionGuid == partition.Guid &&
            partitionEvent.ActorGuid == actor.ActorGuid));
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_Should_CreateDefaultArticle()
    {
        var articleRepository = Substitute.For<IArticleRepository>();
        var handler = CreateDefaultCreateHandler(
            articleRepository,
            actor: CreateActor([ActionType.CreateArticle]));

        await handler.Handle(new ArticleCreateDefaultCommand(
            new Name("Default")));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.ArticleType == ArticleType.Default &&
            !article.IsVariation &&
            !article.IsPack &&
            !article.IsKit &&
            !article.IsContainer));
    }

    [Fact]
    public async Task Create_Should_CreateVariationFromSourceArticle()
    {
        var sourceArticle = Article.CreateArticle(new Name("Source article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(sourceArticle);
        var handler = CreateVariationCreateHandler(
            articleRepository,
            actor: CreateActor([ActionType.CreateArticle]));

        await handler.Handle(new ArticleCreateVariationCommand(
            new Name("Variation"),
            sourceArticle.Guid));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsVariation &&
            article.ArticleType == ArticleType.Variation &&
            article.Variation!.FromArticleGuid == sourceArticle.Guid &&
            article.ModificationTypes == (ArticleModifiedType.General | ArticleModifiedType.Relation)));
    }

    [Fact]
    public async Task Create_Should_CreatePackFromSourceArticle()
    {
        var sourceArticle = Article.CreateArticle(new Name("Source article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(sourceArticle);
        var handler = CreatePackCreateHandler(
            articleRepository,
            actor: CreateActor([ActionType.CreateArticle]));

        await handler.Handle(new ArticleCreatePackCommand(
            new Name("Pack"),
            sourceArticle.Guid,
            4.Amount()));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsPack &&
            article.ArticleType == ArticleType.Pack &&
            article.Pack!.FromArticleGuid == sourceArticle.Guid &&
            article.Pack.Quantity.Equals(4.Amount(), 0.Amount())));
    }

    [Fact]
    public async Task Create_Should_CreateKitFromSourceArticles()
    {
        var firstArticle = Article.CreateArticle(new Name("First article"), CreateDomainActor());
        var secondArticle = Article.CreateArticle(new Name("Second article"), CreateDomainActor());
        var articleRepository = CreateArticleRepository(firstArticle, secondArticle);
        var handler = CreateKitCreateHandler(
            articleRepository,
            actor: CreateActor([ActionType.CreateArticle]));

        await handler.Handle(new ArticleCreateKitCommand(
            new Name("Kit"),
            [
                new ArticleCreateKitPackDto(firstArticle.Guid, 2.Amount()),
                new ArticleCreateKitPackDto(secondArticle.Guid, 3.Amount())
            ]));

        articleRepository.Received(1).Add(Arg.Is<Article>(article =>
            article.IsKit &&
            article.ArticleType == ArticleType.Kit &&
            article.Kit!.Components.Count == 2 &&
            article.ModificationTypes == (ArticleModifiedType.General | ArticleModifiedType.Relation)));
    }

    [Fact]
    public async Task Create_Should_Throw_WhenKitHasNoPacks()
    {
        var handler = CreateKitCreateHandler();

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new ArticleCreateKitCommand(
                new Name("Invalid kit"),
                [])));
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
        Assert.Equal(actor.ActorGuid, article.EditedByActorGuid);
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.General));
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.MetricsChanged));
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.BarcodesChanged));
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.PartitionsChanged));
        Assert.True(article.ModificationTypes.HasFlag(ArticleModifiedType.Deactivated));
        entityEventRepository.Received(1).Add(Arg.Is<EntityEvent>(entityEvent =>
            entityEvent.EventType == EventType.Deactivated &&
            entityEvent.EntityGuid == article.Guid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(partitionEvent =>
            partitionEvent.Event.EventType == EventType.InsertedIntoPartition &&
            partitionEvent.PartitionGuid == newPartition.Guid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(partitionEvent =>
            partitionEvent.Event.EventType == EventType.RemovedFromPartition &&
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
            entityEvent.EventType == EventType.Deleted &&
            entityEvent.EntityType == EntityType.Article &&
            entityEvent.EntityGuid == article.Guid &&
            entityEvent.ActorGuid == actor.ActorGuid));
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    private static ArticleCreateDefaultCommandHandler CreateDefaultCreateHandler(
        IArticleRepository? articleRepository = null,
        IPartitionRepository? partitionRepository = null,
        IEntityEventRepository? entityEventRepository = null,
        IEntityPartitionEventRepository? entityPartitionEventRepository = null,
        IAuthorizationContext? actor = null,
        IUnitOfWork? unitOfWork = null)
    {
        articleRepository ??= Substitute.For<IArticleRepository>();

        return new ArticleCreateDefaultCommandHandler(
            articleRepository,
            partitionRepository ?? Substitute.For<IPartitionRepository>(),
            entityEventRepository ?? Substitute.For<IEntityEventRepository>(),
            entityPartitionEventRepository ?? Substitute.For<IEntityPartitionEventRepository>(),
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor ?? CreateActor([ActionType.CreateArticle])),
            unitOfWork ?? Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticleCreateDefaultCommandHandler>>());
    }

    private static ArticleCreateVariationCommandHandler CreateVariationCreateHandler(
        IArticleRepository? articleRepository = null,
        IPartitionRepository? partitionRepository = null,
        IEntityEventRepository? entityEventRepository = null,
        IEntityPartitionEventRepository? entityPartitionEventRepository = null,
        IAuthorizationContext? actor = null,
        IUnitOfWork? unitOfWork = null)
    {
        articleRepository ??= Substitute.For<IArticleRepository>();

        return new ArticleCreateVariationCommandHandler(
            articleRepository,
            partitionRepository ?? Substitute.For<IPartitionRepository>(),
            entityEventRepository ?? Substitute.For<IEntityEventRepository>(),
            entityPartitionEventRepository ?? Substitute.For<IEntityPartitionEventRepository>(),
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor ?? CreateActor([ActionType.CreateArticle])),
            unitOfWork ?? Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticleCreateVariationCommandHandler>>());
    }

    private static ArticleCreatePackCommandHandler CreatePackCreateHandler(
        IArticleRepository? articleRepository = null,
        IPartitionRepository? partitionRepository = null,
        IEntityEventRepository? entityEventRepository = null,
        IEntityPartitionEventRepository? entityPartitionEventRepository = null,
        IAuthorizationContext? actor = null,
        IUnitOfWork? unitOfWork = null)
    {
        articleRepository ??= Substitute.For<IArticleRepository>();

        return new ArticleCreatePackCommandHandler(
            articleRepository,
            partitionRepository ?? Substitute.For<IPartitionRepository>(),
            entityEventRepository ?? Substitute.For<IEntityEventRepository>(),
            entityPartitionEventRepository ?? Substitute.For<IEntityPartitionEventRepository>(),
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor ?? CreateActor([ActionType.CreateArticle])),
            unitOfWork ?? Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticleCreatePackCommandHandler>>());
    }

    private static ArticleCreateKitCommandHandler CreateKitCreateHandler(
        IArticleRepository? articleRepository = null,
        IPartitionRepository? partitionRepository = null,
        IEntityEventRepository? entityEventRepository = null,
        IEntityPartitionEventRepository? entityPartitionEventRepository = null,
        IAuthorizationContext? actor = null,
        IUnitOfWork? unitOfWork = null)
    {
        articleRepository ??= Substitute.For<IArticleRepository>();

        return new ArticleCreateKitCommandHandler(
            articleRepository,
            partitionRepository ?? Substitute.For<IPartitionRepository>(),
            entityEventRepository ?? Substitute.For<IEntityEventRepository>(),
            entityPartitionEventRepository ?? Substitute.For<IEntityPartitionEventRepository>(),
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor ?? CreateActor([ActionType.CreateArticle])),
            unitOfWork ?? Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticleCreateKitCommandHandler>>());
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
