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

public sealed class CommandNoOpGuardTests
{
    [Fact]
    public async Task ItemSetPartitions_Should_Skip_WhenPartitionsAreUnchanged()
    {
        var partition = new Partition(new Name("Partition"));
        var item = new Item(Article.CreateArticle(new Name("Article")));
        item.AddPartition(partition);
        var itemRepository = Substitute.For<IItemRepository>();
        itemRepository.GetByGuid(item.Guid, Arg.Any<CancellationToken>())
            .Returns(item);
        var partitionRepository = Substitute.For<IPartitionRepository>();
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var handler = new ItemSetPartitionsCommandHandler(
            itemRepository,
            partitionRepository,
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditItem)),
            Substitute.For<ILogger<ItemSetPartitionsCommandHandler>>());

        await handler.Handle(new ItemSetPartitionsCommand(item.Guid, [partition.Guid]));

        await partitionRepository.DidNotReceiveWithAnyArgs()
            .GetByGuid(default, default);
        entityPartitionEventRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task UserSetPermissions_Should_Skip_BeforeOwnPermissionValidation_WhenPermissionsAreUnchanged()
    {
        var actorGuid = Guid.NewGuid();
        var user = new User(new Nameid("valid-user"), new PasswordHash(new string('a', 60)))
        {
            Guid = actorGuid
        };
        user.AddPermission(ActionType.EditUser);
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>())
            .Returns(user);
        var handler = new UserSetPermissionsCommandHandler(
            userRepository,
            CreateCurrentAuthorizationContext(CreateActor(actorGuid, ActionType.EditUser)),
            Substitute.For<ILogger<UserSetPermissionsCommandHandler>>());

        await handler.Handle(new UserSetPermissionsCommand(user.Guid, [ActionType.EditUser]));

        Assert.Single(user.Permissions);
    }

    [Fact]
    public async Task UserGroupSetPartitions_Should_Skip_WhenPartitionsAreUnchanged()
    {
        var partition = new Partition(new Name("Partition"));
        var userGroup = new UserGroup(new Nameid("valid-group"));
        userGroup.AddPartition(partition);
        var userGroupRepository = Substitute.For<IUserGroupRepository>();
        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);
        var partitionRepository = Substitute.For<IPartitionRepository>();
        var entityPartitionEventRepository = Substitute.For<IEntityPartitionEventRepository>();
        var handler = new UserGroupSetPartitionsCommandHandler(
            userGroupRepository,
            partitionRepository,
            entityPartitionEventRepository,
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditUserGroup)),
            Substitute.For<ILogger<UserGroupSetPartitionsCommandHandler>>());

        await handler.Handle(new UserGroupSetPartitionsCommand(userGroup.Guid, [partition.Guid]));

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
