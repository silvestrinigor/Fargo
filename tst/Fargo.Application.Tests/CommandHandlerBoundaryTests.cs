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
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests;

public sealed class CommandHandlerBoundaryTests
{
    [Fact]
    public async Task ItemUpdate_Should_ApplyEndpointPayload_And_SaveOnce()
    {
        var item = Item.CreateItem(Article.NewArticle(new Name("Article"), CreateDomainActor()));
        var parent = Item.CreateItem(Article.CreateArticleContainer(new Name("Container article"), CreateDomainActor()));
        var partition = Partition.CreatePartition(new Name("Partition"));
        var itemRepository = Substitute.For<IItemRepository>();
        itemRepository.GetByGuidAsync(item.Guid, Arg.Any<CancellationToken>())
            .Returns(item);
        itemRepository.GetByGuidAsync(parent.Guid, Arg.Any<CancellationToken>())
            .Returns(parent);
        itemRepository.GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);
        var partitionRepository = CreatePartitionRepository(partition);
        var entityEventRepository = Substitute.For<IEventRepository>();
        var entityPartitionEventRepository = Substitute.For<IPartitionEventRepository>();
        var movementRepository = Substitute.For<IItemMovementRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ItemUpdateCommandHandler(
            itemRepository,
            partitionRepository,
            entityEventRepository,
            entityPartitionEventRepository,
            movementRepository,
            new ItemService(itemRepository),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditItem)),
            unitOfWork,
            Substitute.For<ILogger<ItemUpdateCommandHandler>>());

        await handler.HandleAsync(new ItemUpdateCommand(
            item.Guid,
            new ItemUpdateDto([partition.Guid], parent.Guid, false)));

        Assert.Equal(parent.Guid, item.ParentContainerGuid);
        Assert.Contains(item.Partitions, p => p.Guid == partition.Guid);
        Assert.False(item.IsActive);
        movementRepository.Received(1).Add(Arg.Any<ItemMovement>());
        entityPartitionEventRepository.Received(1).Add(Arg.Any<PartitionEvent>());
        entityEventRepository.Received(1).Add(Arg.Is<Event>(entityEvent =>
            entityEvent.EventType == EventType.EntityDeactivated));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PartitionCreate_Should_SetParent_And_SaveOnce()
    {
        var parent = Partition.CreatePartition(
            PartitionService.GlobalPartitionGuid,
            new Name("Global partition"));
        var partitionRepository = CreatePartitionRepository(parent);
        partitionRepository.GetDescendantGuids(Arg.Any<Guid>(), false, Arg.Any<CancellationToken>())
            .Returns([]);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new PartitionCreateCommandHandler(
            new PartitionService(partitionRepository),
            partitionRepository,
            Substitute.For<IEventRepository>(),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.CreatePartition, ActionType.EditPartition)),
            unitOfWork,
            Substitute.For<ILogger<PartitionCreateCommandHandler>>());

        var partitionGuid = await handler.HandleAsync(new PartitionCreateCommand(
            new PartitionCreateDto(new Name("Partition"))));

        Assert.NotEqual(Guid.Empty, partitionGuid);
        partitionRepository.Received(1).Add(Arg.Is<Partition>(partition =>
            partition.Guid == partitionGuid &&
            partition.ParentPartitionGuid == parent.Guid));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserCreate_Should_ApplyOptionalState_And_SaveOnce()
    {
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.ExistsByNameid(Arg.Any<Nameid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Hash(Arg.Any<string>())
            .Returns(new PasswordHash(new string('a', 60)));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UserCreateCommandHandler(
            new UserService(userRepository),
            userRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IUserGroupRepository>(),
            Substitute.For<IEventRepository>(),
            Substitute.For<IPartitionEventRepository>(),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.CreateUser, ActionType.EditUser)),
            passwordHasher,
            unitOfWork,
            Substitute.For<ILogger<UserCreateCommandHandler>>());

        var userGuid = await handler.HandleAsync(new UserCreateCommand(
            new UserCreateDto(
                "valid-user",
                "ValidPass1!",
                FirstName: new FirstName("Valid"))));

        userRepository.Received(1).Add(Arg.Is<User>(user =>
            user.Guid == userGuid &&
            user.FirstName == new FirstName("Valid")));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserGroupUpdate_Should_ApplyProvidedFields_And_SaveOnce()
    {
        var userGroup = UserGroup.CreateUserGroup(new Nameid("valid-group"));
        var userGroupRepository = Substitute.For<IUserGroupRepository>();
        userGroupRepository.GetByGuid(userGroup.Guid, Arg.Any<CancellationToken>())
            .Returns(userGroup);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UserGroupUpdateCommandHandler(
            new UserGroupService(userGroupRepository),
            userGroupRepository,
            Substitute.For<IPartitionRepository>(),
            Substitute.For<IEventRepository>(),
            Substitute.For<IPartitionEventRepository>(),
            CreateCurrentAuthorizationContext(CreateActor(ActionType.EditUserGroup)),
            unitOfWork,
            Substitute.For<ILogger<UserGroupUpdateCommandHandler>>());

        await handler.HandleAsync(new UserGroupUpdateCommand(
            userGroup.Guid,
            new UserGroupUpdateDto(null, new Description("Updated group"), false, null, null)));

        Assert.Equal(new Description("Updated group"), userGroup.Description);
        Assert.False(userGroup.IsActive);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
