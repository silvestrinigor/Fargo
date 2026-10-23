using Fargo.Application.Articles;
using Fargo.Application.Common;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.Articles;

public class ArticleDeleteCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly TestCurrentActor currentActor = new();

    private ArticleDeleteCommandHandler CreateHandler()
        => new(
            new ArticleService(articleRepository),
            ActorTestFactory.Resolver(userRepository),
            articleRepository,
            auditLogRepository,
            currentActor,
            unitOfWork,
            Substitute.For<ILogger<ArticleDeleteCommandHandler>>());

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldRemoveArticleAndAuditAndSave()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteArticle]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));
        articleRepository.HasItemsAssociatedAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        articleRepository.IsDependencyOfAnotherArticleAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        await CreateHandler().HandleAsync(new ArticleDeleteCommand(article.Guid));

        articleRepository.Received(1).Remove(article);
        auditLogRepository.Received(1).Add(Arg.Any<Fargo.Core.Audits.AuditLog>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ArticleDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        // Actor resolved but without DeleteArticle permission.
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ArticleDeleteCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenArticleNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteArticle]);
        var missing = Guid.NewGuid();
        articleRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(null));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ArticleDeleteCommand(missing)));
    }

    [Fact]
    public async Task HandleAsync_WhenActorHasNoPartitionAccess_ShouldThrowAccessDenied()
    {
        // Actor has the permission but no access to the global partition the
        // article belongs to.
        ActorTestFactory.ConfigureUserActor(
            userRepository, currentActor.Guid, [ActionType.DeleteArticle], partitionAccesses: [Guid.NewGuid()]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        await Assert.ThrowsAsync<ActorAccessDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ArticleDeleteCommand(article.Guid)));
    }

    [Fact]
    public async Task HandleAsync_WhenArticleHasItems_ShouldNotDeleteAndThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.DeleteArticle]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));
        articleRepository.HasItemsAssociatedAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        await Assert.ThrowsAsync<FargoCoreException>(
            () => CreateHandler().HandleAsync(new ArticleDeleteCommand(article.Guid)));

        articleRepository.DidNotReceive().Remove(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
