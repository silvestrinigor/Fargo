using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Requests.Commands.ArticleCommands;

public sealed class ArticleDeleteCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var articleService = new ArticleService(articleRepository);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();

        var handler = new ArticleDeleteCommandHandler(
            articleRepository,
            userRepository,
            articleService,
            unitOfWork,
            currentUser);

        var articleGuid = Guid.NewGuid();
        var actorGuid = Guid.NewGuid();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository
            .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new ArticleDeleteCommand(articleGuid);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await articleRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        articleRepository.DidNotReceive().Remove(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowArticleNotFoundFargoApplicationException_When_ArticleIsNotFound()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var articleService = new ArticleService(articleRepository);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();

        var handler = new ArticleDeleteCommandHandler(
            articleRepository,
            userRepository,
            articleService,
            unitOfWork,
            currentUser);

        var actor = CreateUserWithPermission(ActionType.DeleteArticle);
        var articleGuid = Guid.NewGuid();

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        articleRepository
            .GetByGuid(articleGuid, Arg.Any<CancellationToken>())
            .Returns((Article?)null);

        var command = new ArticleDeleteCommand(articleGuid);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<ArticleNotFoundFargoApplicationException>(act);

        articleRepository.DidNotReceive().Remove(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var articleService = new ArticleService(articleRepository);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();

        var handler = new ArticleDeleteCommandHandler(
            articleRepository,
            userRepository,
            articleService,
            unitOfWork,
            currentUser);

        var actor = CreateUser();
        var article = CreateArticle();
        var command = new ArticleDeleteCommand(article.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        articleRepository
            .GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        articleRepository.DidNotReceive().Remove(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowArticleDeleteWithItemsAssociatedFargoDomainException_When_ArticleHasAssociatedItems()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var articleService = new ArticleService(articleRepository);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();

        var handler = new ArticleDeleteCommandHandler(
            articleRepository,
            userRepository,
            articleService,
            unitOfWork,
            currentUser);

        var actor = CreateUserWithPermission(ActionType.DeleteArticle);
        var article = CreateArticle();
        var command = new ArticleDeleteCommand(article.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        articleRepository
            .GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);

        articleRepository
            .HasItemsAssociated(article.Guid, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<ArticleDeleteWithItemsAssociatedFargoDomainException>(act);

        articleRepository.DidNotReceive().Remove(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveArticleAndSaveChanges_When_RequestIsValid()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var articleService = new ArticleService(articleRepository);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();

        var handler = new ArticleDeleteCommandHandler(
            articleRepository,
            userRepository,
            articleService,
            unitOfWork,
            currentUser);

        var actor = CreateUserWithPermission(ActionType.DeleteArticle);
        var article = CreateArticle();
        var command = new ArticleDeleteCommand(article.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);

        articleRepository
            .GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);

        articleRepository
            .HasItemsAssociated(article.Guid, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await handler.Handle(command);

        // Assert
        articleRepository.Received(1).Remove(article);
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var articleService = new ArticleService(articleRepository);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUser>();

        var handler = new ArticleDeleteCommandHandler(
            articleRepository,
            userRepository,
            articleService,
            unitOfWork,
            currentUser);

        var actor = CreateUserWithPermission(ActionType.DeleteArticle);
        var article = CreateArticle();
        var cancellationToken = new CancellationTokenSource().Token;
        var command = new ArticleDeleteCommand(article.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        articleRepository
            .GetByGuid(article.Guid, cancellationToken)
            .Returns(article);

        articleRepository
            .HasItemsAssociated(article.Guid, cancellationToken)
            .Returns(false);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1).GetByGuid(actor.Guid, cancellationToken);
        await articleRepository.Received(1).GetByGuid(article.Guid, cancellationToken);
        await articleRepository.Received(1).HasItemsAssociated(article.Guid, cancellationToken);
        await unitOfWork.Received(1).SaveChanges(cancellationToken);
    }

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("igor123"),
            PasswordHash = new PasswordHash(new string('a', PasswordHash.MinLength))
        };
    }

    private static User CreateUserWithPermission(ActionType action)
    {
        var user = CreateUser();
        user.AddPermission(action);
        return user;
    }

    private static Article CreateArticle()
    {
        return new Article
        {
            Guid = Guid.NewGuid(),
            Name = new Name("Notebook")
        };
    }
}