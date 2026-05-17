using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests;

public sealed class EntityPartitionEventCommandHandlerTests
{
    [Fact]
    public async Task ArticleSetPartitions_Should_RecordInsertedPartitionEvent()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var partition = Partition.CreatePartition(new Name("Partition"));
        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);
        var partitionRepository = CreatePartitionRepository(partition);
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var actor = CreateActor(ActionType.EditArticle);
        var handler = new ArticleSetPartitionsCommandHandler(
            articleRepository,
            partitionRepository,
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ArticleSetPartitionsCommandHandler>>());

        await handler.Handle(new ArticleSetPartitionsCommand(article.Guid, [partition.Guid]));

        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EntityEventType.InsertedIntoPartition &&
            entityPartitionEvent.EntityType == EntityType.Article &&
            entityPartitionEvent.EntityGuid == article.Guid &&
            entityPartitionEvent.PartitionGuid == partition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task ItemSetPartitions_Should_RecordRemovedAndInsertedPartitionEvents()
    {
        var oldPartition = Partition.CreatePartition(new Name("Old partition"));
        var newPartition = Partition.CreatePartition(new Name("New partition"));
        var item = Item.CreateItem(Article.CreateArticle(new Name("Article")));
        item.AddPartition(oldPartition);
        var itemRepository = Substitute.For<IItemRepository>();
        itemRepository.GetByGuid(item.Guid, Arg.Any<CancellationToken>())
            .Returns(item);
        var partitionRepository = CreatePartitionRepository(newPartition);
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var actor = CreateActor(ActionType.EditItem);
        var handler = new ItemSetPartitionsCommandHandler(
            itemRepository,
            partitionRepository,
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<ItemSetPartitionsCommandHandler>>());

        await handler.Handle(new ItemSetPartitionsCommand(item.Guid, [newPartition.Guid]));

        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EntityEventType.InsertedIntoPartition &&
            entityPartitionEvent.EntityType == EntityType.Item &&
            entityPartitionEvent.EntityGuid == item.Guid &&
            entityPartitionEvent.PartitionGuid == newPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EntityEventType.RemovedFromPartition &&
            entityPartitionEvent.EntityType == EntityType.Item &&
            entityPartitionEvent.EntityGuid == item.Guid &&
            entityPartitionEvent.PartitionGuid == oldPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task UserSetPartitions_Should_RecordRemovedAndInsertedPartitionEvents()
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
        var handler = new UserSetPartitionsCommandHandler(
            userRepository,
            partitionRepository,
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<UserSetPartitionsCommandHandler>>());

        await handler.Handle(new UserSetPartitionsCommand(user.Guid, [newPartition.Guid]));

        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EntityEventType.InsertedIntoPartition &&
            entityPartitionEvent.EntityType == EntityType.User &&
            entityPartitionEvent.EntityGuid == user.Guid &&
            entityPartitionEvent.PartitionGuid == newPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EntityEventType.RemovedFromPartition &&
            entityPartitionEvent.EntityType == EntityType.User &&
            entityPartitionEvent.EntityGuid == user.Guid &&
            entityPartitionEvent.PartitionGuid == oldPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
    }

    [Fact]
    public async Task UserGroupSetPartitions_Should_RecordRemovedAndInsertedPartitionEvents()
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
        var handler = new UserGroupSetPartitionsCommandHandler(
            userGroupRepository,
            partitionRepository,
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<UserGroupSetPartitionsCommandHandler>>());

        await handler.Handle(new UserGroupSetPartitionsCommand(userGroup.Guid, [newPartition.Guid]));

        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EntityEventType.InsertedIntoPartition &&
            entityPartitionEvent.EntityType == EntityType.UserGroup &&
            entityPartitionEvent.EntityGuid == userGroup.Guid &&
            entityPartitionEvent.PartitionGuid == newPartition.Guid &&
            entityPartitionEvent.ActorGuid == actor.ActorGuid));
        entityPartitionEventRepository.Received(1).Add(Arg.Is<EntityPartitionEvent>(entityPartitionEvent =>
            entityPartitionEvent.Event.EventType == EntityEventType.RemovedFromPartition &&
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
