using Fargo.Domain.Partitions;
using Fargo.Domain.System;
using Fargo.Domain.UserGroups;
using Fargo.Domain.Users;
using NSubstitute;

namespace Fargo.Domain.Tests.Actors;

public sealed class ActorServiceTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();

    [Fact]
    public async Task GetActorByGuid_Should_ReturnSystemActorWithAllActionsAndGlobalDescendantPartitions()
    {
        var partitionGuids = new[]
        {
            PartitionService.GlobalPartitionGuid,
            Guid.NewGuid(),
            Guid.NewGuid()
        };
        partitionRepository
            .GetDescendantGuids(PartitionService.GlobalPartitionGuid, true, Arg.Any<CancellationToken>())
            .Returns(partitionGuids);
        var sut = new ActorService(userRepository, partitionRepository);

        var actor = await sut.GetActorByGuid(SystemService.SystemGuid);

        Assert.NotNull(actor);
        Assert.True(actor.IsSystem);
        Assert.True(actor.IsAdmin);
        Assert.Equal(Enum.GetValues<ActionType>().Order(), actor.PermissionActions.Order());
        Assert.Equal(partitionGuids.Order(), actor.PartitionAccessesGuids.Order());
        await partitionRepository.Received(1)
            .GetDescendantGuids(PartitionService.GlobalPartitionGuid, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetActorByGuid_Should_ReturnAdminUserActorUsingDatabasePermissionsAndExpandedPartitionAccess()
    {
        var globalPartition = CreatePartition(PartitionService.GlobalPartitionGuid, "Global");
        var directPartition = CreatePartition(Guid.NewGuid(), "Direct partition");
        var childPartitionGuid = Guid.NewGuid();
        var user = new User
        {
            Guid = UserService.DefaultAdministratorUserGuid,
            Nameid = new Nameid("admin"),
            PasswordHash = CreatePasswordHash()
        };
        user.AddPermission(ActionType.CreateArticle);
        user.AddPartitionAccess(globalPartition);
        user.AddPartitionAccess(directPartition);

        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>()).Returns(user);
        partitionRepository
            .GetDescendantGuids(
                Arg.Is<IReadOnlyCollection<Guid>>(guids =>
                    guids.Count == 2 &&
                    guids.Contains(globalPartition.Guid) &&
                    guids.Contains(directPartition.Guid)),
                true,
                Arg.Any<CancellationToken>())
            .Returns([globalPartition.Guid, directPartition.Guid, childPartitionGuid]);
        var sut = new ActorService(userRepository, partitionRepository);

        var actor = await sut.GetActorByGuid(user.Guid);

        Assert.NotNull(actor);
        Assert.True(actor.IsAdmin);
        Assert.False(actor.IsSystem);
        Assert.Equal([ActionType.CreateArticle], actor.PermissionActions);
        Assert.Equal(
            new[] { globalPartition.Guid, directPartition.Guid, childPartitionGuid }.Order(),
            actor.PartitionAccessesGuids.Order());
    }

    [Fact]
    public async Task GetActorByGuid_Should_ReturnUserActorUsingDirectAndGroupPermissionsAndExpandedPartitionAccess()
    {
        var directPartition = CreatePartition(Guid.NewGuid(), "Direct partition");
        var groupPartition = CreatePartition(Guid.NewGuid(), "Group partition");
        var childPartitionGuid = Guid.NewGuid();
        var group = new UserGroup { Nameid = new Nameid("operators") };
        group.AddPermission(ActionType.DeleteItem);
        group.AddPartitionAccess(groupPartition);
        var user = new User
        {
            Nameid = new Nameid("operator"),
            PasswordHash = CreatePasswordHash()
        };
        user.AddPermission(ActionType.CreateItem);
        user.AddPartitionAccess(directPartition);
        user.UserGroups.Add(group);

        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>()).Returns(user);
        partitionRepository
            .GetDescendantGuids(
                Arg.Is<IReadOnlyCollection<Guid>>(guids =>
                    guids.Count == 2 &&
                    guids.Contains(directPartition.Guid) &&
                    guids.Contains(groupPartition.Guid)),
                true,
                Arg.Any<CancellationToken>())
            .Returns([directPartition.Guid, groupPartition.Guid, childPartitionGuid]);
        var sut = new ActorService(userRepository, partitionRepository);

        var actor = await sut.GetActorByGuid(user.Guid);

        Assert.NotNull(actor);
        Assert.False(actor.IsAdmin);
        Assert.Equal(
            new[] { ActionType.CreateItem, ActionType.DeleteItem }.Order(),
            actor.PermissionActions.Order());
        Assert.Equal(
            new[] { directPartition.Guid, groupPartition.Guid, childPartitionGuid }.Order(),
            actor.PartitionAccessesGuids.Order());
    }

    [Fact]
    public async Task GetActorByGuid_Should_IgnoreInactiveGroupPermissionsAndPartitionAccess()
    {
        var directPartition = CreatePartition(Guid.NewGuid(), "Direct partition");
        var groupPartition = CreatePartition(Guid.NewGuid(), "Group partition");
        var group = new UserGroup { Nameid = new Nameid("inactive-operators") };
        group.AddPermission(ActionType.DeleteItem);
        group.AddPartitionAccess(groupPartition);
        group.Deactivate();
        var user = new User
        {
            Nameid = new Nameid("operator"),
            PasswordHash = CreatePasswordHash()
        };
        user.AddPermission(ActionType.CreateItem);
        user.AddPartitionAccess(directPartition);
        user.UserGroups.Add(group);

        userRepository.GetByGuid(user.Guid, Arg.Any<CancellationToken>()).Returns(user);
        partitionRepository
            .GetDescendantGuids(
                Arg.Is<IReadOnlyCollection<Guid>>(guids =>
                    guids.Count == 1 &&
                    guids.Contains(directPartition.Guid)),
                true,
                Arg.Any<CancellationToken>())
            .Returns([directPartition.Guid]);
        var sut = new ActorService(userRepository, partitionRepository);

        var actor = await sut.GetActorByGuid(user.Guid);

        Assert.NotNull(actor);
        Assert.Equal([ActionType.CreateItem], actor.PermissionActions);
        Assert.Equal([directPartition.Guid], actor.PartitionAccessesGuids);
    }

    private static Partition CreatePartition(Guid guid, string name)
        => new()
        {
            Guid = guid,
            Name = new Name(name)
        };

    private static PasswordHash CreatePasswordHash()
        => new(new string('h', PasswordHash.MinLength));
}
