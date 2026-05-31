using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Shared.Articles;
using Fargo.Application.Shared.Items;
using Fargo.Application.Shared.UserGroups;
using Fargo.Application.Shared.Users;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests;

public sealed class EntityPartitionEventCommandHandlerTests
{
    [Fact]
    public async Task ArticlePatchPartitions_Should_RecordInsertedPartitionEvent()
    {
        var article = Article.CreateArticle(new Name("Article"), CreateDomainActor());
        var partition = Partition.CreatePartition(new Name("Partition"));
        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);
        var partitionRepository = CreatePartitionRepository(partition);
        var entityEventRepository = Substitute.For<IEntityEventRepository>();
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticlePatchCommandHandler(
            articleRepository,
            partitionRepository,
            entityEventRepository,
            entityPartitionEventRepository,
            new ArticleService(articleRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ArticlePatchCommandHandler>>());

        await handler.Handle(new ArticlePatchCommand(
            article.Guid,
            new ArticlePatchDto(Partitions: [partition.Guid])));

        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EventType.InsertedIntoPartition &&
            entityPartitionEvent.EntityType == EntityType.Article &&
            entityPartitionEvent.EntityGuid == article.Guid &&
            entityPartitionEvent.PartitionGuid == partition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task ItemUpdatePartitions_Should_RecordRemovedAndInsertedPartitionEvents()
    {
        var oldPartition = Partition.CreatePartition(new Name("Old partition"));
        var newPartition = Partition.CreatePartition(new Name("New partition"));
        var item = Item.CreateItem(Article.CreateArticle(new Name("Article"), CreateDomainActor()));
        item.AddPartition(oldPartition);
        var itemRepository = Substitute.For<IItemRepository>();
        itemRepository.GetByGuid(item.Guid, Arg.Any<CancellationToken>())
            .Returns(item);
        var partitionRepository = CreatePartitionRepository(newPartition);
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            partitionRepository,
            Substitute.For<IEntityEventRepository>(),
            entityPartitionEventRepository,
            Substitute.For<IItemMovementRepository>(),
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.Handle(new ItemUpdateCommand(item.Guid, new ItemUpdateDto([newPartition.Guid])));

        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EventType.InsertedIntoPartition &&
            entityPartitionEvent.EntityType == EntityType.Item &&
            entityPartitionEvent.EntityGuid == item.Guid &&
            entityPartitionEvent.PartitionGuid == newPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EventType.RemovedFromPartition &&
            entityPartitionEvent.EntityType == EntityType.Item &&
            entityPartitionEvent.EntityGuid == item.Guid &&
            entityPartitionEvent.PartitionGuid == oldPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task UserUpdatePartitions_Should_RecordRemovedAndInsertedPartitionEvents()
    {
        var oldPartition = Partition.CreatePartition(new Name("Old partition"));
        var newPartition = Partition.CreatePartition(new Name("New partition"));
        var user = User.CreateUser(new Nameid("valid-user"), new PasswordHash(new string('a', 60)));
        user.AddPartition(oldPartition);
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);
        var partitionRepository = CreatePartitionRepository(newPartition);
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var actor = CreateActor(ActionType.EditUser);
        var handler = new UserUpdateCommandHandler(
            new UserService(userRepository),
            userRepository,
            partitionRepository,
            Substitute.For<IUserGroupRepository>(),
            Substitute.For<IRefreshTokenRepository>(),
            Substitute.For<IEntityEventRepository>(),
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IPasswordHasher>(),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<UserUpdateCommandHandler>>());

        await handler.Handle(new UserUpdateCommand(
            user.Guid,
            new UserUpdateDto(Partitions: [newPartition.Guid])));

        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EventType.InsertedIntoPartition &&
            entityPartitionEvent.EntityType == EntityType.User &&
            entityPartitionEvent.EntityGuid == user.Guid &&
            entityPartitionEvent.PartitionGuid == newPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EventType.RemovedFromPartition &&
            entityPartitionEvent.EntityType == EntityType.User &&
            entityPartitionEvent.EntityGuid == user.Guid &&
            entityPartitionEvent.PartitionGuid == oldPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task UserGroupUpdatePartitions_Should_RecordRemovedAndInsertedPartitionEvents()
    {
        var oldPartition = Partition.CreatePartition(new Name("Old partition"));
        var newPartition = Partition.CreatePartition(new Name("New partition"));
        var userGroup = UserGroup.CreateUserGroup(new Nameid("valid-group"));
        userGroup.AddPartition(oldPartition);
        var userGroupRepository = Substitute.For<IUserGroupRepository>();
        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);
        var partitionRepository = CreatePartitionRepository(newPartition);
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var actor = CreateActor(ActionType.EditUserGroup);
        var handler = new UserGroupUpdateCommandHandler(
            new UserGroupService(userGroupRepository),
            userGroupRepository,
            partitionRepository,
            Substitute.For<IEntityEventRepository>(),
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<UserGroupUpdateCommandHandler>>());

        await handler.Handle(new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateDto(null, null, null, null, [newPartition.Guid])));

        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EventType.InsertedIntoPartition &&
            entityPartitionEvent.EntityType == EntityType.UserGroup &&
            entityPartitionEvent.EntityGuid == userGroup.Guid &&
            entityPartitionEvent.PartitionGuid == newPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EventType.RemovedFromPartition &&
            entityPartitionEvent.EntityType == EntityType.UserGroup &&
            entityPartitionEvent.EntityGuid == userGroup.Guid &&
            entityPartitionEvent.PartitionGuid == oldPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
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

    private static IAuthorizationContext CreateActor(params ActionType[] permissions)
        => new AuthorizationContext(
            Guid.NewGuid(),
            IsAuthenticated: true,
            IsAdmin: true,
            permissions,
            PartitionAccesses: [],
            UserGroupGuids: []);
}
