using Fargo.Core.UserGroups;

namespace Fargo.Core.Tests.UserGroups;

public class UserGroupTests
{
    [Fact]
    public void CreateAdministratorsUserGroup_WhenValid_ShouldBeAdministrators()
    {
        var administrators = UserGroup.CreateAdministratorsUserGroup(default);

        Assert.Equal(FargoCoreWellKnowGuids.AdminUserGroupGuid, administrators.Guid);
        Assert.True(administrators.IsAdministrators);
    }

    [Fact]
    public void CreateNormalUserGroup_WhenValid_ShouldNotBeAdministrators()
    {
        var userGroup = UserGroup.CreateUserGroup(default);

        Assert.NotEqual(FargoCoreWellKnowGuids.AdminUserGroupGuid, userGroup.Guid);
        Assert.False(userGroup.IsAdministrators);
    }
}
