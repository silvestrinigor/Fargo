using Fargo.Application.Exceptions;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.ArticleCommands;

public sealed class ArticleCreateCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new ArticleCreateCommandHandler(
            articleRepository,
            userRepository,
            currentUser,
            unitOfWork);

        var command = new ArticleCreateCommand(
            new ArticleCreateModel(new Name("Notebook"))
        );

        var userGuid = Guid.NewGuid();

        currentUser.UserGuid.Returns(userGuid);

        userRepository
            .GetByGuid(userGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        articleRepository.DidNotReceive().Add(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new ArticleCreateCommandHandler(
            articleRepository,
            userRepository,
            currentUser,
            unitOfWork);

        var command = new ArticleCreateCommand(
            new ArticleCreateModel(new Name("Notebook"))
        );

        var actor = CreateUser();
        var userGuid = actor.Guid;

        currentUser.UserGuid.Returns(userGuid);

        userRepository
            .GetByGuid(userGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        articleRepository.DidNotReceive().Add(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AddArticleAndSaveChanges_When_ActorHasPermission()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new ArticleCreateCommandHandler(
            articleRepository,
            userRepository,
            currentUser,
            unitOfWork);

        var command = new ArticleCreateCommand(
            new ArticleCreateModel(
                new Name("Notebook"),
                new Description("A high-quality notebook for writing and drawing"
                ))
        );

        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var userGuid = actor.Guid;

        currentUser.UserGuid.Returns(userGuid);

        userRepository
            .GetByGuid(userGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        Article? addedArticle = null;

        articleRepository
            .When(x => x.Add(Arg.Any<Article>()))
            .Do(callInfo => addedArticle = callInfo.Arg<Article>());

        // Act
        var result = await handler.Handle(command);

        // Assert
        articleRepository.Received(1).Add(Arg.Any<Article>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedArticle);
        Assert.Equal(command.Article.Name, addedArticle!.Name);
        Assert.Equal(addedArticle.Guid, result);
        Assert.Equal(command.Article.Description, addedArticle.Description);
    }

    [Fact]
    public async Task Handle_Should_UseCurrentUserGuidToLoadActor()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new ArticleCreateCommandHandler(
            articleRepository,
            userRepository,
            currentUser,
            unitOfWork);

        var command = new ArticleCreateCommand(
            new ArticleCreateModel(new Name("Notebook"))
        );

        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var userGuid = actor.Guid;

        currentUser.UserGuid.Returns(userGuid);

        userRepository
            .GetByGuid(userGuid, Arg.Any<CancellationToken>())
            .Returns(actor);

        // Act
        await handler.Handle(command);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(userGuid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var articleRepository = Substitute.For<IArticleRepository>();
        var userRepository = Substitute.For<IUserRepository>();
        var currentUser = Substitute.For<ICurrentUser>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new ArticleCreateCommandHandler(
            articleRepository,
            userRepository,
            currentUser,
            unitOfWork);

        var command = new ArticleCreateCommand(
            new ArticleCreateModel(new Name("Notebook"))
        );

        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var cancellationToken = new CancellationTokenSource().Token;

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
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
}