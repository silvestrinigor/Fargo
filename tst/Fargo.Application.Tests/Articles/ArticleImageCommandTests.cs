using Fargo.Application.Articles;
using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Events;
using Fargo.Domain.Partitions;
using Fargo.Domain.System;
using Fargo.Domain.Users;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fargo.Application.Tests.Articles;

public sealed class ArticleImageCommandTests
{
    [Fact]
    public async Task Upload_ShouldDeleteNewImage_WhenDatabaseCommitFails()
    {
        var article = CreateArticle();
        article.Images.ImageKey = "articles/old.png";

        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);

        var imageStorage = Substitute.For<IArticleImageStorage>();
        imageStorage.SaveAsync(article.Guid, Arg.Any<Stream>(), "image/png", Arg.Any<CancellationToken>())
            .Returns("articles/new.png");

        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.SaveChanges(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("db write failed"));

        var sut = new ArticleImageUploadCommandHandler(
            CreateActorService(),
            articleRepository,
            imageStorage,
            unitOfWork,
            new StubCurrentUser());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(new ArticleImageUploadCommand(article.Guid, new MemoryStream([1, 2, 3]), "image/png")));

        Assert.Equal("articles/old.png", article.Images.ImageKey);
        await imageStorage.Received(1).DeleteAsync("articles/new.png", Arg.Any<CancellationToken>());
        await imageStorage.DidNotReceive().DeleteAsync("articles/old.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Upload_ShouldDeletePreviousImage_AfterDatabaseCommit()
    {
        var article = CreateArticle();
        article.Images.ImageKey = "articles/old.png";

        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);

        var imageStorage = Substitute.For<IArticleImageStorage>();
        imageStorage.SaveAsync(article.Guid, Arg.Any<Stream>(), "image/png", Arg.Any<CancellationToken>())
            .Returns("articles/new.png");

        var unitOfWork = Substitute.For<IUnitOfWork>();

        var sut = new ArticleImageUploadCommandHandler(
            CreateActorService(),
            articleRepository,
            imageStorage,
            unitOfWork,
            new StubCurrentUser());

        await sut.Handle(new ArticleImageUploadCommand(article.Guid, new MemoryStream([1, 2, 3]), "image/png"));

        Assert.Equal("articles/new.png", article.Images.ImageKey);
        Received.InOrder(async () =>
        {
            await imageStorage.SaveAsync(article.Guid, Arg.Any<Stream>(), "image/png", Arg.Any<CancellationToken>());
            await unitOfWork.SaveChanges(Arg.Any<CancellationToken>());
            await imageStorage.DeleteAsync("articles/old.png", Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task DeleteImage_ShouldDeleteStorage_AfterDatabaseCommit()
    {
        var article = CreateArticle();
        article.Images.ImageKey = "articles/existing.png";

        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);

        var imageStorage = Substitute.For<IArticleImageStorage>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var sut = new ArticleImageDeleteCommandHandler(
            CreateActorService(),
            articleRepository,
            imageStorage,
            unitOfWork,
            new StubCurrentUser());

        await sut.Handle(new ArticleImageDeleteCommand(article.Guid));

        Assert.Null(article.Images.ImageKey);
        Received.InOrder(async () =>
        {
            await unitOfWork.SaveChanges(Arg.Any<CancellationToken>());
            await imageStorage.DeleteAsync("articles/existing.png", Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task DeleteArticle_ShouldDeleteStorage_AfterDatabaseCommit()
    {
        var article = CreateArticle();
        article.Images.ImageKey = "articles/existing.png";

        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);
        articleRepository.HasItemsAssociated(article.Guid, Arg.Any<CancellationToken>())
            .Returns(false);

        var imageStorage = Substitute.For<IArticleImageStorage>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var eventRecorder = Substitute.For<IEventRecorder>();
        var eventPublisher = Substitute.For<IFargoEventPublisher>();

        var sut = new ArticleDeleteCommandHandler(
            CreateActorService(),
            new Domain.Articles.ArticleService(articleRepository),
            articleRepository,
            imageStorage,
            unitOfWork,
            new StubCurrentUser(),
            eventRecorder,
            eventPublisher);

        await sut.Handle(new ArticleDeleteCommand(article.Guid));

        Received.InOrder(async () =>
        {
            await eventRecorder.Record(EventType.ArticleDeleted, EntityType.Article, article.Guid, Arg.Any<CancellationToken>());
            await unitOfWork.SaveChanges(Arg.Any<CancellationToken>());
            await imageStorage.DeleteAsync("articles/existing.png", Arg.Any<CancellationToken>());
            await eventPublisher.PublishArticleDeleted(article.Guid, Arg.Any<CancellationToken>());
        });
    }

    private static ActorService CreateActorService()
        => new(
            Substitute.For<IUserRepository>(),
            Substitute.For<IPartitionRepository>());

    private static Article CreateArticle()
        => new()
        {
            Name = new Name("Test article"),
        };

    private sealed class StubCurrentUser : ICurrentUser
    {
        public Guid UserGuid => SystemService.SystemGuid;

        public bool IsAuthenticated => true;
    }
}
