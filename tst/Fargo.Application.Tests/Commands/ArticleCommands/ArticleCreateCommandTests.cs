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

public sealed class ArticleCreateCommandHandlerTests
{
    private static readonly Name articleName = new("Notebook");
    private static readonly Description articleDescription =
        new("A high-quality notebook for writing and drawing");

    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    private readonly PartitionService partitionService;
    private readonly ArticleCreateCommandHandler handler;

    public ArticleCreateCommandHandlerTests()
    {
        partitionService = new PartitionService(partitionRepository);

        handler = new ArticleCreateCommandHandler(
            partitionService,
            articleRepository,
            userRepository,
            currentUser,
            unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsNotFound()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var command = CreateCommand();

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
    public async Task Handle_Should_ThrowUnauthorizedAccessFargoApplicationException_When_ActorIsInactive()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        actor.IsActive = false;

        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

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
        var actor = CreateUser();
        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

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
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var command = CreateCommand(articleDescription);
        Article? addedArticle = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        CaptureAddedArticle(article => addedArticle = article);

        // Act
        var result = await handler.Handle(command);

        // Assert
        articleRepository.Received(1).Add(Arg.Any<Article>());
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());

        Assert.NotNull(addedArticle);
        Assert.Equal(command.Article.Name, addedArticle!.Name);
        Assert.Equal(command.Article.Description, addedArticle.Description);
        Assert.Equal(addedArticle.Guid, result);
    }

    [Fact]
    public async Task Handle_Should_UseDescriptionEmpty_When_DescriptionIsNull()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var command = CreateCommand(description: null);
        Article? addedArticle = null;

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        CaptureAddedArticle(article => addedArticle = article);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedArticle);
        Assert.Equal(Description.Empty, addedArticle!.Description);
    }

    [Fact]
    public async Task Handle_Should_AddFirstPartition_When_FirstPartitionExistsAndActorHasAccess()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var partition = CreatePartition();
        var command = CreateCommand(firstPartition: partition.Guid);
        Article? addedArticle = null;

        actor.AddPartitionAccess(
            partition
            );

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);
        CaptureAddedArticle(article => addedArticle = article);

        partitionRepository
            .GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);

        // Act
        await handler.Handle(command);

        // Assert
        Assert.NotNull(addedArticle);
        Assert.Contains(
            addedArticle!.Partitions,
            x => x.Guid == partition.Guid);
    }

    [Fact]
    public async Task Handle_Should_ThrowPartitionNotFoundFargoApplicationException_When_FirstPartitionDoesNotExist()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var partitionGuid = Guid.NewGuid();
        var command = CreateCommand(firstPartition: partitionGuid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        partitionRepository
            .GetByGuid(partitionGuid, Arg.Any<CancellationToken>())
            .Returns((Partition?)null);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<PartitionNotFoundFargoApplicationException>(act);

        articleRepository.DidNotReceive().Add(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowPartitionNotFoundFargoApplicationException_When_ActorDoesNotHaveAccessToFirstPartition()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var partition = CreatePartition();
        var command = CreateCommand(firstPartition: partition.Guid);

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        partitionRepository
            .GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);

        // Act
        Task act() => handler.Handle(command);

        // Assert
        await Assert.ThrowsAsync<PartitionNotFoundFargoApplicationException>(act);

        articleRepository.DidNotReceive().Add(Arg.Any<Article>());
        await unitOfWork.DidNotReceive().SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseCurrentUserGuidToLoadActor()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var command = CreateCommand();

        ConfigureCurrentUser(actor);
        ConfigureActorLookup(actor);

        // Act
        await handler.Handle(command);

        // Assert
        await userRepository.Received(1)
            .GetByGuid(actor.Guid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var command = CreateCommand();
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

    [Fact]
    public async Task Handle_Should_UseProvidedCancellationToken_ForPartitionLookup()
    {
        // Arrange
        var actor = CreateUserWithPermission(ActionType.CreateArticle);
        var partition = CreatePartition();
        var command = CreateCommand(firstPartition: partition.Guid);
        var cancellationToken = new CancellationTokenSource().Token;

        actor.AddPartitionAccess(partition);

        currentUser.UserGuid.Returns(actor.Guid);

        userRepository
            .GetByGuid(actor.Guid, cancellationToken)
            .Returns(actor);

        partitionRepository
            .GetByGuid(partition.Guid, cancellationToken)
            .Returns(partition);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await partitionRepository.Received(1)
            .GetByGuid(partition.Guid, cancellationToken);
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

    private void CaptureAddedArticle(Action<Article> capture)
    {
        articleRepository
            .When(x => x.Add(Arg.Any<Article>()))
            .Do(callInfo => capture(callInfo.Arg<Article>()));
    }

    private static ArticleCreateCommand CreateCommand(
        Description? description = null,
        Guid? firstPartition = null)
    {
        return new ArticleCreateCommand(
            new ArticleCreateModel(articleName, description, firstPartition));
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

    private static Partition CreatePartition()
    {
        return new Partition
        {
            Guid = Guid.NewGuid(),
            Name = new Name(new string('a', Name.MinLength)),
            Description = new Description(new string('a', Description.MinLength)),
            IsActive = true,
            IsGlobal = false,
            IsEditable = true
        };
    }
}