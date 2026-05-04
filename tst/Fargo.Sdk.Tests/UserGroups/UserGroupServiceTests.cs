using Fargo.Api.UserGroups;
using NSubstitute;

namespace Fargo.Api.Tests.UserGroups;

public sealed class UserGroupServiceTests
{
    private readonly IUserGroupHttpClient client = Substitute.For<IUserGroupHttpClient>();
    private readonly UserGroupService sut;

    public UserGroupServiceTests()
    {
        sut = new UserGroupService(client, Substitute.For<IFargoEventHub>());
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnUserGroup_When_UserGroupExists()
    {
        var result = Fakes.UserGroupResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserGroupResult>(result));

        var userGroup = await sut.GetAsync(result.Guid);

        Assert.Equal(result.Guid, userGroup.Guid);
        Assert.Equal(result.Nameid, userGroup.Nameid);
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_UserGroupNotFound()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserGroupResult>(new FargoSdkError(FargoSdkErrorType.NotFound, "User group not found.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserGroupResult>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnUserGroups_When_UserGroupsExist()
    {
        var results = new[] { Fakes.UserGroupResult(), Fakes.UserGroupResult() };
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<UserGroupResult>>(results));

        var userGroups = await sut.GetManyAsync();

        Assert.Equal(2, userGroups.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_NoUserGroupsExist()
    {
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<UserGroupResult>>([]));

        var userGroups = await sut.GetManyAsync();

        Assert.Empty(userGroups);
    }

    [Fact]
    public async Task GetManyAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<UserGroupResult>>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetManyAsync());
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnUserGroup_When_CreationSucceeds()
    {
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var userGroup = await sut.CreateAsync("mygroup", "A description");

        Assert.Equal(guid, userGroup.Guid);
        Assert.Equal("mygroup", userGroup.Nameid);
        Assert.Equal("A description", userGroup.Description);
    }

    [Fact]
    public async Task CreateAsync_Should_UseEmptyDescription_When_DescriptionIsNull()
    {
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var userGroup = await sut.CreateAsync("mygroup");

        Assert.Equal(string.Empty, userGroup.Description);
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowFargoSdkApiException_When_CreationFails()
    {
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Nameid is required.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.CreateAsync(string.Empty));
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_CallClient_When_UserGroupExists()
    {
        var guid = Guid.NewGuid();
        client.DeleteAsync(guid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        await sut.DeleteAsync(guid);

        await client.Received(1).DeleteAsync(guid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowFargoSdkApiException_When_DeleteFails()
    {
        client.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Cannot delete default admin group.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.DeleteAsync(Guid.NewGuid()));
    }

    // --- Entity tracking ---

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var result = Fakes.UserGroupResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserGroupResult>(result));

        var userGroup = await sut.GetAsync(result.Guid);

        UserGroupUpdatedEventArgs? received = null;
        userGroup.Updated += (_, e) => received = e;

        userGroup.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseDeleted_FiresEntityDeletedEvent()
    {
        var result = Fakes.UserGroupResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserGroupResult>(result));

        var userGroup = await sut.GetAsync(result.Guid);

        UserGroupDeletedEventArgs? received = null;
        userGroup.Deleted += (_, e) => received = e;

        userGroup.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task CreateAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var userGroup = await sut.CreateAsync("mygroup");

        UserGroupUpdatedEventArgs? received = null;
        userGroup.Updated += (_, e) => received = e;

        userGroup.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(guid, received.Guid);
    }

    private static class Fakes
    {
        public static UserGroupResult UserGroupResult() =>
            new(Guid.NewGuid(), "testgroup", "A test description", true, []);
    }
}
