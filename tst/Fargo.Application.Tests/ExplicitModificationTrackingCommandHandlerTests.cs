using Fargo.Application.Identity;
using Fargo.Application.Items;
using Fargo.Application.Partitions;
using Fargo.Application.Workspaces;
using Fargo.Application.UserGroups;
using Fargo.Application.Users;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Identity;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
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
        var article = Article.CreateArticle(new Name("Article"));
        var itemRepository = Substitute.For<IItemRepository>();
        var articleRepository = Substitute.For<IArticleRepository>();
        articleRepository.GetByGuid(article.Guid, Arg.Any<CancellationToken>())
            .Returns(article);
        var actor = CreateActor(ActionType.CreateItem);
        var reservedGuidSession = new ReservedGuidSession();
        var reservedItemGuid = reservedGuidSession.ReserveItemGuid();
        var handler = new ItemCreateCommandHandler(
            itemRepository,
            articleRepository,
            Substitute.For<IEntityEventRepository>(),
            CreateCurrentAuthorizationContext(actor),
            reservedGuidSession,
            Substitute.For<ILogger<ItemCreateCommandHandler>>());

        await handler.Handle(new ItemCreateCommand(article.Guid, ItemGuid: reservedItemGuid));

        itemRepository.Received(1).Add(Arg.Is<Item>(item =>
            item.EditedByGuid == actor.ActorGuid &&
            item.ModificationTypes == ItemModifiedType.General));
    }

    [Fact]
    public async Task PartitionRename_Should_MarkActorAndGeneralModificationType()
    {
        var partition = Partition.CreatePartition(new Name("Partition"));
        var repository = Substitute.For<IPartitionRepository>();
        repository.GetByGuid(partition.Guid, Arg.Any<CancellationToken>())
            .Returns(partition);
        var actor = CreateActor(ActionType.EditPartition);
        var handler = new PartitionRenameCommandHandler(
            repository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<PartitionRenameCommandHandler>>());

        await handler.Handle(new PartitionRenameCommand(partition.Guid, new Name("Renamed")));

        Assert.Equal(actor.ActorGuid, partition.EditedByGuid);
        Assert.Equal(PartitionModifiedType.General, partition.ModificationTypes);
    }

    [Fact]
    public async Task UserChangePassword_Should_MarkActorAndPasswordModificationType()
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
        var handler = new UserChangePasswordCommandHandler(
            userRepository,
            passwordHasher,
            refreshTokenRepository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<UserChangePasswordCommandHandler>>());

        await handler.Handle(new UserChangePasswordCommand(user.Guid, new Password("ValidPass1!")));

        Assert.Equal(actor.ActorGuid, user.EditedByGuid);
        Assert.Equal(UserModifiedType.PasswordChanged, user.ModificationTypes);
    }

    [Fact]
    public async Task UserGroupSetPermissions_Should_MarkActorAndPermissionsModificationType()
    {
        var userGroup = UserGroup.CreateUserGroup(new Nameid("valid-group"));
        var repository = Substitute.For<IUserGroupRepository>();
        repository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);
        var actor = CreateActor(ActionType.EditUserGroup);
        var handler = new UserGroupSetPermissionsCommandHandler(
            repository,
            CreateCurrentAuthorizationContext(actor),
            Substitute.For<ILogger<UserGroupSetPermissionsCommandHandler>>());

        await handler.Handle(new UserGroupSetPermissionsCommand(userGroup.Guid, [ActionType.CreateUser]));

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
