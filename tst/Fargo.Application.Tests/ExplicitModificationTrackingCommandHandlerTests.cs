using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.Shared.Items;
using Fargo.Application.Shared.Partitions;
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

public sealed class ExplicitModificationTrackingCommandHandlerTests
{
    [Fact]
    public async Task ItemCreate_Should_MarkActorAndGeneralModificationType()
    {
        var article = Article.CreateArticle(new Name("Article"), CreateDomainActor());
        var itemRepository = Substitute.For<IItemRepository>();
        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);
        var actor = CreateActor(ActionType.CreateItem);
        var handler = new ItemCreateCommandHandler(
            itemRepository,
            articleRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IEntityEventRepository>(),
            Substitute.For<IEntityPartitionEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemCreateCommandHandler>>());

        await handler.Handle(new ItemCreateCommand(new ItemCreateDto(article.Guid)));

        itemRepository.Received(1).Add(Arg.Is<Item>(item =>
            item.EditedByGuid == actor.ActorGuid &&
            item.ModificationTypes == ItemModifiedType.General));
    }

    [Fact]
    public async Task PartitionUpdateName_Should_MarkActorAndGeneralModificationType()
    {
        var partition = Partition.CreatePartition(new Name("Partition"));
        var repository = Substitute.For<IPartitionRepository>();
        repository.GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);
        var actor = CreateActor(ActionType.EditPartition);
        var handler = new PartitionUpdateCommandHandler(
            new PartitionService(repository),
            repository,
            Substitute.For<IEntityEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<PartitionUpdateCommandHandler>>());

        await handler.Handle(new PartitionUpdateCommand(
            partition.Guid,
            new PartitionUpdateDto(Name: new Name("Renamed"))));

        Assert.Equal(actor.ActorGuid, partition.EditedByGuid);
        Assert.Equal(PartitionModifiedType.General, partition.ModificationTypes);
    }

    [Fact]
    public async Task UserUpdatePassword_Should_MarkActorAndPasswordModificationType()
    {
        var user = User.CreateUser(new Nameid("valid-user"), new PasswordHash(new string('a', 60)));
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);
        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Hash(Arg.Any<string>())
            .Returns(new PasswordHash(new string('b', 60)));
        var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        refreshTokenRepository.GetByUserGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns([]);
        var actor = CreateActor(ActionType.EditUser, ActionType.ChangeOtherUserPassword);
        var handler = new UserUpdateCommandHandler(
            new UserService(userRepository),
            userRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IUserGroupRepository>(),
            refreshTokenRepository,
            Substitute.For<IEntityEventRepository>(),
            Substitute.For<IEntityPartitionEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            passwordHasher,
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<UserUpdateCommandHandler>>());

        await handler.Handle(new UserUpdateCommand(user.Guid, new UserUpdateDto(Password: "ValidPass1!")));

        Assert.Equal(actor.ActorGuid, user.EditedByGuid);
        Assert.Equal(UserModifiedType.PasswordChanged, user.ModificationTypes);
    }

    [Fact]
    public async Task UserGroupUpdatePermissions_Should_MarkActorAndPermissionsModificationType()
    {
        var userGroup = UserGroup.CreateUserGroup(new Nameid("valid-group"));
        var repository = Substitute.For<IUserGroupRepository>();
        repository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);
        var actor = CreateActor(ActionType.EditUserGroup);
        var handler = new UserGroupUpdateCommandHandler(
            new UserGroupService(repository),
            repository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IEntityEventRepository>(),
            Substitute.For<IEntityPartitionEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<UserGroupUpdateCommandHandler>>());

        await handler.Handle(new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateDto(null, null, null, [new UserGroupPermissionUpdateDto(ActionType.CreateUser)], null)));

        Assert.Equal(actor.ActorGuid, userGroup.EditedByGuid);
        Assert.Equal(UserGroupModifiedType.PermissionsChanged, userGroup.ModificationTypes);
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
