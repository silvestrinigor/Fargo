using Fargo.Core.UserGroups;
using NSubstitute;

namespace Fargo.Core.Tests.UserGroups;

public class UserGroupServiceTests
{
    private readonly IUserGroupRepository userGroupRepository;
    private readonly UserGroupService userGroupService;

    public UserGroupServiceTests()
    {
        userGroupRepository = Substitute.For<IUserGroupRepository>();
        userGroupService = new UserGroupService(userGroupRepository);
    }

    [Fact]
    public async Task InsertIntoUserGroupAsync_WhenUserGroupIsValid_ShouldSetParentUserGroup()
    {
        var userGroup1 = UserGroup.CreateUserGroup(default);
        var userGroup2 = UserGroup.CreateUserGroup(default);
        userGroupRepository
        .GetDescendantUserGroupGuidsAsync(userGroup2.Guid, includeRoot: false, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        await userGroupService.ValidateParentUserGroupAssignmentAsync(userGroup1, userGroup2);
    }

    [Fact]
    public async Task InsertIntoUserGroupAsync_WhenCreatesCircularHierarchy_ShouldThrowException()
    {
        var userGroup1 = UserGroup.CreateUserGroup(default);
        var userGroup2 = UserGroup.CreateUserGroup(default);
        var userGroup3 = UserGroup.CreateUserGroup(default);
        userGroup2.SetParentUserGroup(userGroup1);
        userGroup3.SetParentUserGroup(userGroup2);
        userGroupRepository
        .GetDescendantUserGroupGuidsAsync(userGroup1.Guid, includeRoot: false, Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([userGroup2.Guid, userGroup3.Guid]));

        async Task function() => await userGroupService.ValidateParentUserGroupAssignmentAsync(userGroup3, userGroup1);

        var ex = await Assert.ThrowsAsync<FargoCoreException>(function);
        Assert.Equal(FargoCoreErrorType.InvalidOperation, ex.ErrorType);
    }
}
