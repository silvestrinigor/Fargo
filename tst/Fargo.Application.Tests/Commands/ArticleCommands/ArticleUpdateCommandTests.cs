using Fargo.Application.Exceptions;
using Fargo.Application.Models.ArticleModels;
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

namespace Fargo.Application.Tests.Commands.ArticleCommands;

public sealed class ArticleUpdateCommandHandlerTests
{
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();

    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();

    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly ArticleService articleService;

    private readonly ArticleUpdateCommandHandler handler;

    public ArticleUpdateCommandHandlerTests()
    {
        articleService = new(articleRepository);

        handler = new ArticleUpdateCommandHandler(
                articleService,
                articleRepository,
                userRepository,
                unitOfWork,
                currentUser);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var actorGuid = Guid.NewGuid();
        var command = CreateCommand();

        currentUser.UserGuid.Returns(actorGuid);

        userRepository
            .GetByGuid(actorGuid, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await articleRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var actor = CreateUser();
        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        await articleRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowArticleNotFoundFargoApplicationException_When_ArticleIsNotFound()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditArticle);
        var articleGuid = Guid.NewGuid();
        var command = CreateCommand(articleGuid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        articleRepository
            .GetByGuid(articleGuid, Arg.Any<CancellationToken>())
            .Returns((Article?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<ArticleNotFoundFargoApplicationException>(act);

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateArticleName_When_NameIsProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditArticle);
        var article = CreateArticle();
        var newName = new Name("Updated Notebook");

        var command = CreateCommand(
                article.Guid,
                new ArticleUpdateModel(Name: newName));

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureArticleLookup(article);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newName.Value, article.Name);
        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateArticleDescription_When_DescriptionIsProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditArticle);
        var article = CreateArticle();
        var newDescription = new Description("Updated article description.");

        var command = CreateCommand(
                article.Guid,
                new ArticleUpdateModel(Description: newDescription));

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureArticleLookup(article);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newDescription.Value, article.Description);
        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateArticleNameAndDescription_When_BothAreProvided()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditArticle);
        var article = CreateArticle();

        var newName = new Name("Updated Notebook");
        var newDescription = new Description("Updated article description.");

        var command = CreateCommand(
                article.Guid,
                new ArticleUpdateModel(
                    Name: newName,
                    Description: newDescription));

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureArticleLookup(article);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(newName.Value, article.Name);
        Assert.Equal(newDescription.Value, article.Description);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotChangeArticle_When_UpdateModelHasNoValues()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditArticle);
        var article = CreateArticle();

        var originalName = article.Name;
        var originalDescription = article.Description;

        var command = CreateCommand(
                article.Guid,
                new ArticleUpdateModel());

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureArticleLookup(article);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.Equal(originalName, article.Name);
        Assert.Equal(originalDescription, article.Description);

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditArticle);
        var article = CreateArticle();
        var cancellationToken = new CancellationTokenSource().Token;

        var command = CreateCommand(article.Guid);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        articleRepository
            .GetByGuid(article.Guid, cancellationToken)
            .Returns(article);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, cancellationToken);

        await articleRepository.Received(1)
            .GetByGuid(article.Guid, cancellationToken);

        await unitOfWork.Received(1)
            .SaveChanges(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsInactive()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.EditArticle);
        actor.IsActive = false;

        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessFargoApplicationException>(act);

        await articleRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    private void ConfigureCurrentUser(User actor)
    {
        currentUser.UserGuid.Returns(actor.Guid);
    }

    private void ConfigureActorLookup(User actor)
    {
        userRepository
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(actor);
    }

    private void ConfigureArticleLookup(Article article)
    {
        articleRepository
            .GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);
    }

    private static ArticleUpdateCommand CreateCommand(
            Guid? articleGuid = null,
            ArticleUpdateModel? model = null)
        => new(
                articleGuid ?? Guid.NewGuid(),
                model ?? new ArticleUpdateModel());

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("user123"),
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
            Name = new Name("Notebook"),
            Description = new Description("Original article description.")
        };
    }
}