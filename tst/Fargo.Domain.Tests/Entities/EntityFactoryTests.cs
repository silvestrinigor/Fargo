using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;

namespace Fargo.Core.Tests.Entities;

public sealed class EntityFactoryTests
{
    [Fact]
    public void CreatePartition_Should_SetInitialState()
    {
        var description = new Description("Partition description");

        var partition = Partition.CreatePartition(new Name("Partition"), description);

        Assert.Equal("Partition", partition.Name.Value);
        Assert.Equal(description, partition.Description);
        Assert.True(partition.IsActive);
    }

    [Fact]
    public void CreatePartition_WithGuid_Should_SetInitialStateAndGuid()
    {
        var partitionGuid = Guid.NewGuid();

        var partition = Partition.CreatePartition(partitionGuid, new Name("Partition"));

        Assert.Equal(partitionGuid, partition.Guid);
        Assert.Equal("Partition", partition.Name.Value);
    }

    [Fact]
    public void CreateUser_Should_SetInitialState()
    {
        var nameid = new Nameid("valid-user");
        var passwordHash = new PasswordHash(new string('a', 60));

        var user = User.CreateUser(nameid, passwordHash);

        Assert.Equal(nameid, user.Nameid);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void CreateUser_WithGuid_Should_SetInitialStateAndGuid()
    {
        var userGuid = Guid.NewGuid();
        var nameid = new Nameid("valid-user");
        var passwordHash = new PasswordHash(new string('a', 60));

        var user = User.CreateUser(userGuid, nameid, passwordHash);

        Assert.Equal(userGuid, user.Guid);
        Assert.Equal(nameid, user.Nameid);
    }

    [Fact]
    public void CreateUserGroup_Should_SetInitialState()
    {
        var nameid = new Nameid("valid-group");
        var description = new Description("Group description");

        var userGroup = UserGroup.CreateUserGroup(nameid, description);

        Assert.Equal(nameid, userGroup.Nameid);
        Assert.Equal(description, userGroup.Description);
        Assert.True(userGroup.IsActive);
    }

    [Fact]
    public void CreateUserGroup_WithGuid_Should_SetInitialStateAndGuid()
    {
        var userGroupGuid = Guid.NewGuid();
        var nameid = new Nameid("valid-group");

        var userGroup = UserGroup.CreateUserGroup(userGroupGuid, nameid);

        Assert.Equal(userGroupGuid, userGroup.Guid);
        Assert.Equal(nameid, userGroup.Nameid);
    }
}
