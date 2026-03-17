using Fargo.Application.Exceptions;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using NSubstitute;

namespace Fargo.Application.Tests.Commands.ItemCommands;

public sealed class ItemCreateCommandHandlerTests
{
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    private readonly ItemCreateCommandHandler handler;

    public ItemCreateCommandHandlerTests()
    {
        handler = new ItemCreateCommandHandler(
            itemRepository,
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
        var articleGuid = Guid.NewGuid();
        var command = CreateCommand(articleGuid);

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

        itemRepository.DidNotReceive()
            .Add(Arg.Any<Item>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowArticleNotFoundFargoApplicationException_When_ArticleIsNotFound()
    {
        // Arrange
        var actor = CreateUser();
        actor.AddPermission(ActionType.CreateItem);
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

        itemRepository.DidNotReceive()
            .Add(Arg.Any<Item>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowUserNotAuthorizedFargoDomainException_When_ActorDoesNotHavePermission()
    {
        // Arrange
        var actor = CreateUser();
        var article = CreateArticle();
        var command = CreateCommand(article.Guid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureArticleLookup(article);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<UserNotAuthorizedFargoDomainException>(act);

        itemRepository.DidNotReceive()
            .Add(Arg.Any<Item>());

        await unitOfWork.DidNotReceive()
            .SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_AddItemAndSaveChanges_When_RequestIsValid()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateItem);
        var article = CreateArticle();
        var command = CreateCommand(article.Guid);

        Item? addedItem = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        ConfigureArticleLookup(article);
        CaptureAddedItem(item => addedItem = item);

        // Act
        var result = await handler.Handle(command);

        // Assert
        itemRepository.Received(1)
            .Add(Arg.Any<Item>());

        await unitOfWork.Received(1)
            .SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedItem);
        Assert.Equal(article, addedItem!.Article);
        Assert.Equal(addedItem.Guid, result);
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateItem);
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
    public async Task Handle_Should_ThrowUserInactiveFargoDomainException_When_ActorIsInactive()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateItem);
        actor.IsActive = false;

        var article = CreateArticle();
        var command = CreateCommand(article.Guid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        var exception = await Assert.ThrowsAsync<UserInactiveFargoDomainException>(act);
        Assert.Equal(actor.Guid, exception.UserGuid);

        await articleRepository.DidNotReceive()
            .GetByGuid(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        itemRepository.DidNotReceive()
            .Add(Arg.Any<Item>());

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

    private void CaptureAddedItem(Action<Item> capture)
    {
        itemRepository
            .When(x => x.Add(Arg.Any<Item>()))
            .Do(callInfo => capture(callInfo.Arg<Item>()));
    }

    private static ItemCreateCommand CreateCommand(Guid articleGuid)
        => new(new ItemCreateModel(articleGuid));

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
            Description = new Description("Article used for item creation.")
        };
    }
}
