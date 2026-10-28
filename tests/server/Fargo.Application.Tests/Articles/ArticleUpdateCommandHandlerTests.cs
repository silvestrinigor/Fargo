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

public class ArticleUpdateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ArticleUpdateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            new ArticleService(articleRepository),
            articleRepository,
            partitionRepository,
            auditLogRepository,
            currentActor,
            unitOfWork);

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldUpdateArticleAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditArticle]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        var command = new ArticleUpdateCommand(article.Guid, new ArticleUpdateDto(Name: new Name("Renamed")));

        await CreateHandler().HandleAsync(command);

        Assert.Equal(new Name("Renamed"), article.Name);
        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ArticleUpdateCommand(Guid.NewGuid(), new ArticleUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ArticleUpdateCommand(Guid.NewGuid(), new ArticleUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenArticleNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditArticle]);
        var missing = Guid.NewGuid();
        articleRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ArticleUpdateCommand(missing, new ArticleUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.EditArticle], partitionAccesses: [Guid.NewGuid()]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ArticleUpdateCommand(article.Guid, new ArticleUpdateDto())));
    }

    [Fact]
    public async Task HandleAsync_WhenEan13AlreadyAssigned_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.EditArticle]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));
        var ean13 = new Ean13("0000000000000");
        articleRepository.ExistsByEan13Async(ean13, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var command = new ArticleUpdateCommand(
            article.Guid, new ArticleUpdateDto(Barcode: new ArticleBarcodeUpdateDto(Ean13: ean13)));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(command));

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
