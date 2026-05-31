using Fargo.Application.Identity;
using Fargo.Application.Items;
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

public sealed class CommandNoOpGuardTests
{
    [Fact]
    public async Task ItemUpdate_Should_SkipPartitions_WhenPartitionsAreUnchanged()
    {
        var partition = Partition.CreatePartition(new Name("Partition"));
        var item = Item.CreateItem(Article.CreateArticle(new Name("Article"), CreateDomainActor()));
        item.AddPartition(partition);
        var itemRepository = Substitute.For<IItemRepository>();
        itemRepository.GetByGuid(item.Guid, Arg.Any<CancellationToken>())
            .Returns(item);
        var partitionRepository = Substitute.For<IPartitionRepository>();
        var entityEventRepository = Substitute.For<IEntityEventRepository>();
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            partitionRepository,
            entityEventRepository,
            entityPartitionEventRepository,
            Substitute.For<IItemMovementRepository>(),
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditItem)),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.Handle(new ItemUpdateCommand(item.Guid, new ItemUpdateDto([partition.Guid])));

        await partitionRepository.DidNotReceiveWithAnyArgs()
            .GetByGuid(default, default);
        entityPartitionEventRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task UserUpdate_Should_SkipOwnPermissionValidation_WhenPermissionsAreUnchanged()
    {
        var actorGuid = Guid.NewGuid();
        var user = User.CreateUser(actorGuid, new Nameid("valid-user"), new PasswordHash(new string('a', 60)));
        user.AddPermission(ActionType.EditUser);
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);
        var handler = new UserUpdateCommandHandler(
            new UserService(userRepository),
            userRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IUserGroupRepository>(),
            Substitute.For<IRefreshTokenRepository>(),
            Substitute.For<IEntityEventRepository>(),
            Substitute.For<IEntityPartitionEventRepository>(),
            CreateCurrentAuthorizationContext(CreateActor(actorGuid, ActionType.EditUser)),
            Substitute.For<IPasswordHasher>(),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<UserUpdateCommandHandler>>());

        await handler.Handle(new UserUpdateCommand(
            user.Guid,
            new UserUpdateDto(Permissions: [new UserPermissionUpdateDto(ActionType.EditUser)])));

        Assert.Single(user.Permissions);
    }

    [Fact]
    public async Task UserGroupUpdate_Should_SkipPartitions_WhenPartitionsAreUnchanged()
    {
        var partition = Partition.CreatePartition(new Name("Partition"));
        var userGroup = UserGroup.CreateUserGroup(new Nameid("valid-group"));
        userGroup.AddPartition(partition);
        var userGroupRepository = Substitute.For<IUserGroupRepository>();
        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);
        var partitionRepository = Substitute.For<IPartitionRepository>();
        var entityEventRepository = Substitute.For<IEntityEventRepository>();
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var handler = new UserGroupUpdateCommandHandler(
            new UserGroupService(userGroupRepository),
            userGroupRepository,
            partitionRepository,
            entityEventRepository,
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditUserGroup)),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<ILogger<UserGroupUpdateCommandHandler>>());

        await handler.Handle(new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateDto(null, null, null, null, [partition.Guid])));

        await partitionRepository.DidNotReceiveWithAnyArgs()
            .GetByGuid(default, default);
        entityPartitionEventRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    private static ICurrentAuthorizationContext CreateCurrentAuthorizationContext(IAuthorizationContext actor)
    {
        var currentAuthorizationContext = Substitute.For<ICurrentAuthorizationContext>();
        currentAuthorizationContext.GetAsync(Arg.Any<CancellationToken>())
            .Returns(actor);

        return currentAuthorizationContext;
    }

    private static IAuthorizationContext CreateActor(params ActionType[] permissions)
        => CreateActor(Guid.NewGuid(), permissions);

    private static IAuthorizationContext CreateActor(Guid actorGuid, params ActionType[] permissions)
        => new AuthorizationContext(
            actorGuid,
            IsAuthenticated: true,
            IsAdmin: true,
            permissions,
            PartitionAccesses: [],
            UserGroupGuids: []);
}
