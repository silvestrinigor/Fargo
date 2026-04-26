using Fargo.Sdk.Events;
using Fargo.Sdk.Users;
using NSubstitute;

namespace Fargo.Sdk.Tests.Users;

public sealed class UserServiceTests
{
    private readonly IUserHttpClient client = Substitute.For<IUserHttpClient>();
    private readonly UserService sut;

    public UserServiceTests()
    {
        sut = new UserService(client, Substitute.For<IFargoEventHub>());
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnUser_When_UserExists()
    {
        var result = Fakes.UserResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserResult>(result));

        var user = await sut.GetAsync(result.Guid);

        Assert.Equal(result.Guid, user.Guid);
        Assert.Equal(result.Nameid, user.Nameid);
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_UserNotFound()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserResult>(new FargoSdkError(FargoSdkErrorType.NotFound, "User not found.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserResult>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnUsers_When_UsersExist()
    {
        var results = new[] { Fakes.UserResult(), Fakes.UserResult() };
        client.GetManyAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<UserResult>>(results));

        var users = await sut.GetManyAsync();

        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_NoUsersExist()
    {
        client.GetManyAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<UserResult>>([]));

        var users = await sut.GetManyAsync();

        Assert.Empty(users);
    }

    [Fact]
    public async Task GetManyAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetManyAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<UserResult>>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetManyAsync());
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnUser_When_CreationSucceeds()
    {
        var guid = Guid.NewGuid();
        var userResult = Fakes.UserResult(guid, "newuser");
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<TimeSpan?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));
        client.GetAsync(guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserResult>(userResult));

        var user = await sut.CreateAsync("newuser", "password");

        Assert.Equal(guid, user.Guid);
        Assert.Equal("newuser", user.Nameid);
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowFargoSdkApiException_When_CreationFails()
    {
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<TimeSpan?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Nameid is required.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.CreateAsync(string.Empty, "password"));
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_CallClient_When_UserExists()
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
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Cannot delete self.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.DeleteAsync(Guid.NewGuid()));
    }

    // --- Entity tracking ---

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var result = Fakes.UserResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserResult>(result));

        var user = await sut.GetAsync(result.Guid);

        UserUpdatedEventArgs? received = null;
        user.Updated += (_, e) => received = e;

        user.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseDeleted_FiresEntityDeletedEvent()
    {
        var result = Fakes.UserResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserResult>(result));

        var user = await sut.GetAsync(result.Guid);

        UserDeletedEventArgs? received = null;
        user.Deleted += (_, e) => received = e;

        user.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task CreateAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var guid = Guid.NewGuid();
        var userResult = Fakes.UserResult(guid);
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<TimeSpan?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));
        client.GetAsync(guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<UserResult>(userResult));

        var user = await sut.CreateAsync("newuser", "password");

        UserUpdatedEventArgs? received = null;
        user.Updated += (_, e) => received = e;

        user.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(guid, received.Guid);
    }

    private static class Fakes
    {
        public static UserResult UserResult(Guid? guid = null, string nameid = "testuser") =>
            new(guid ?? Guid.NewGuid(), nameid, null, null, string.Empty,
                TimeSpan.Zero, DateTimeOffset.UtcNow.AddYears(1), true, [], []);
    }
}
