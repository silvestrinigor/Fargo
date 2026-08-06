using Fargo.Core.Users;

namespace Fargo.Core.Tests.Users;

public class UserTests
{
    [Fact]
    public void CreateAdminUser_WhenValid_ShouldBeAdminUser()
    {
        var admin = User.CreateAdministratorUser(default, default);

        Assert.Equal(FargoCoreWellKnowGuids.AdminUserGuid, admin.Guid);
        Assert.True(admin.IsAdmin);
    }

    [Fact]
    public void CreateNormalUser_WhenValid_ShouldNotBeAdminUser()
    {
        var user = User.CreateUser(default);

        Assert.NotEqual(FargoCoreWellKnowGuids.AdminUserGuid, user.Guid);
        Assert.False(user.IsAdmin);
    }
}
