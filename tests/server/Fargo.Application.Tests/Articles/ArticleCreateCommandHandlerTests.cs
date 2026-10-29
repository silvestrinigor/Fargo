using Fargo.Application.Articles;
using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Barcodes;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.Users;
using NSubstitute;

namespace Fargo.Application.Tests.Articles;

public class ArticleCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ArticleCreateCommandHandler CreateHandler()
        => new(
            new ArticleService(articleRepository),
            ActorTestFactory.Resolver(userRepository),
            articleRepository,
            partitionRepository,
            auditLogRepository,
            currentActor);

    private static ArticleCreateCommand DefaultCommand()
        => new(new ArticleCreateDto(new Name("Widget")));

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldAddArticleAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateArticle]);

        var result = await CreateHandler().HandleAsync(DefaultCommand());

        articleRepository.Received(1).Add(Arg.Is<Article>(a => a.Guid == result));
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task HandleAsync_WhenTrackingModeNotSpecified_ShouldCreateEntityTrackedArticle()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateArticle]);

        await CreateHandler().HandleAsync(DefaultCommand());

        articleRepository.Received(1).Add(
            Arg.Is<Article>(a => a.TrackingMode == ArticleTrackingMode.Entity));
    }

    [Fact]
    public async Task HandleAsync_WhenTrackingModeIsEvent_ShouldCreateEventTrackedArticle()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateArticle]);

        var command = new ArticleCreateCommand(new ArticleCreateDto(
            new Name("Bulk sand"),
            TrackingMode: ArticleTrackingMode.Event));

        await CreateHandler().HandleAsync(command);

        articleRepository.Received(1).Add(
            Arg.Is<Article>(a =>
                a.TrackingMode == ArticleTrackingMode.Event &&
                a.ArticleType == ArticleType.Default));
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(DefaultCommand()));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(DefaultCommand()));
    }

    [Fact]
    public async Task HandleAsync_WhenVariationArticleNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateArticle]);
        var missing = Guid.NewGuid();
        articleRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(null));

        var command = new ArticleCreateCommand(new ArticleCreateDto(
            new Name("Variant"),
            ArticleType: ArticleType.Variation,
            Variation: new ArticleVariationDto(missing)));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoAccessToFromArticle_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.CreateArticle], partitionAccesses: [Guid.NewGuid()]);
        var fromArticle = Article.NewArticle(new Name("Base"));
        articleRepository.GetByGuidAsync(fromArticle.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(fromArticle));

        var command = new ArticleCreateCommand(new ArticleCreateDto(
            new Name("Variant"),
            ArticleType: ArticleType.Variation,
            Variation: new ArticleVariationDto(fromArticle.Guid)));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenVariationConfigurationMissing_ShouldThrowApplicationException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateArticle]);

        var command = new ArticleCreateCommand(new ArticleCreateDto(
            new Name("Variant"),
            ArticleType: ArticleType.Variation,
            Variation: null));

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenEan13AlreadyAssigned_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateArticle]);
        var ean13 = new Ean13("0000000000000");
        articleRepository.ExistsByEan13Async(ean13, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var command = new ArticleCreateCommand(new ArticleCreateDto(
            new Name("Widget"),
            Barcode: new ArticleBarcodeDto(ean13)));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(command));

        articleRepository.DidNotReceive().Add(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
