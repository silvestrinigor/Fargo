using Fargo.Sdk.Partitions;
using Fargo.Sdk.Users;
using NSubstitute;

namespace Fargo.Sdk.Tests.Users;

public sealed class UserTests
{
    private static readonly Guid UserGuid = Guid.NewGuid();
    private readonly IUserHttpClient client = Substitute.For<IUserHttpClient>();
    private readonly User sut;

    public UserTests()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        sut = new User(UserGuid, "testuser", null, null, "desc", TimeSpan.Zero, DateTimeOffset.UtcNow.AddYears(1), true, [], [], client);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_CallClientUpdateAsync_With_ProvidedValues()
    {
        await sut.UpdateAsync(x => x.Nameid = "newuser");

        await client.Received(1).UpdateAsync(UserGuid, "newuser", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Should_BatchMultipleChangesIntoOneRequest()
    {
        await sut.UpdateAsync(x =>
        {
            x.Nameid = "newuser";
            x.FirstName = "John";
        });

        await client.Received(1).UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateLocalValues()
    {
        await sut.UpdateAsync(x =>
        {
            x.Nameid = "newuser";
            x.FirstName = "John";
            x.Description = "Updated desc";
        });

        Assert.Equal("newuser", sut.Nameid);
        Assert.Equal("John", sut.FirstName);
        Assert.Equal("Updated desc", sut.Description);
    }

    [Fact]
    public async Task UpdateAsync_Should_ThrowFargoSdkApiException_When_UpdateFails()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Nameid is required.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.UpdateAsync(x => x.Nameid = string.Empty));
    }

    // --- Property getters ---

    [Fact]
    public void Nameid_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.Nameid = "newuser";

        Assert.Equal("newuser", sut.Nameid);
    }

    [Fact]
    public void IsActive_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.IsActive = false;

        Assert.False(sut.IsActive);
    }

    [Fact]
    public void Guid_Should_ReturnGuidPassedAtConstruction()
    {
        Assert.Equal(UserGuid, sut.Guid);
    }

    // --- Updated event ---

    [Fact]
    public void Updated_Should_Fire_When_RaiseUpdatedIsCalled()
    {
        UserUpdatedEventArgs? received = null;
        sut.Updated += (_, e) => received = e;

        sut.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(UserGuid, received.Guid);
    }

    [Fact]
    public void Updated_Should_NotFire_When_NoHandlerIsAttached()
    {
        sut.RaiseUpdated();
    }

    // --- Deleted event ---

    [Fact]
    public void Deleted_Should_Fire_When_RaiseDeletedIsCalled()
    {
        UserDeletedEventArgs? received = null;
        sut.Deleted += (_, e) => received = e;

        sut.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(UserGuid, received.Guid);
    }

    [Fact]
    public void Deleted_Should_NotFire_When_NoHandlerIsAttached()
    {
        sut.RaiseDeleted();
    }

    // --- GetPartitionsAsync ---

    [Fact]
    public async Task GetPartitionsAsync_Should_DelegateToClient()
    {
        IReadOnlyCollection<PartitionResult> partitions = [new PartitionResult(Guid.NewGuid(), "P1", "desc", null, true)];
        client.GetPartitionsAsync(UserGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(partitions));

        var result = await sut.GetPartitionsAsync();

        await client.Received(1).GetPartitionsAsync(UserGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    // --- AddUserGroupAsync / RemoveUserGroupAsync ---

    [Fact]
    public async Task AddUserGroupAsync_Should_DelegateToClient()
    {
        var groupGuid = Guid.NewGuid();
        client.AddUserGroupAsync(UserGuid, groupGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        var result = await sut.AddUserGroupAsync(groupGuid);

        await client.Received(1).AddUserGroupAsync(UserGuid, groupGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RemoveUserGroupAsync_Should_DelegateToClient()
    {
        var groupGuid = Guid.NewGuid();
        client.RemoveUserGroupAsync(UserGuid, groupGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        var result = await sut.RemoveUserGroupAsync(groupGuid);

        await client.Received(1).RemoveUserGroupAsync(UserGuid, groupGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
    }
}
