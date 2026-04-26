using Fargo.Sdk.Partitions;
using Fargo.Sdk.UserGroups;
using NSubstitute;

namespace Fargo.Sdk.Tests.UserGroups;

public sealed class UserGroupTests
{
    private static readonly Guid UserGroupGuid = Guid.NewGuid();
    private readonly IUserGroupHttpClient client = Substitute.For<IUserGroupHttpClient>();
    private readonly UserGroup sut;

    public UserGroupTests()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        sut = new UserGroup(UserGroupGuid, "testgroup", "Original Description", true, [], client);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_CallClientUpdateAsync_With_ProvidedValues()
    {
        await sut.UpdateAsync(x => x.Nameid = "newgroup");

        await client.Received(1).UpdateAsync(UserGroupGuid, "newgroup", Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Should_BatchMultipleChangesIntoOneRequest()
    {
        await sut.UpdateAsync(x =>
        {
            x.Nameid = "newgroup";
            x.Description = "New Description";
        });

        await client.Received(1).UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateLocalValues()
    {
        await sut.UpdateAsync(x =>
        {
            x.Nameid = "newgroup";
            x.Description = "New Description";
        });

        Assert.Equal("newgroup", sut.Nameid);
        Assert.Equal("New Description", sut.Description);
    }

    [Fact]
    public async Task UpdateAsync_Should_ThrowFargoSdkApiException_When_UpdateFails()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<IReadOnlyCollection<ActionType>?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Nameid is required.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.UpdateAsync(x => x.Nameid = string.Empty));
    }

    // --- Property getters ---

    [Fact]
    public void Nameid_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.Nameid = "newgroup";

        Assert.Equal("newgroup", sut.Nameid);
    }

    [Fact]
    public void Description_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.Description = "New Description";

        Assert.Equal("New Description", sut.Description);
    }

    [Fact]
    public void Guid_Should_ReturnGuidPassedAtConstruction()
    {
        Assert.Equal(UserGroupGuid, sut.Guid);
    }

    // --- Updated event ---

    [Fact]
    public void Updated_Should_Fire_When_RaiseUpdatedIsCalled()
    {
        UserGroupUpdatedEventArgs? received = null;
        sut.Updated += (_, e) => received = e;

        sut.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(UserGroupGuid, received.Guid);
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
        UserGroupDeletedEventArgs? received = null;
        sut.Deleted += (_, e) => received = e;

        sut.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(UserGroupGuid, received.Guid);
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
        client.GetPartitionsAsync(UserGroupGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(partitions));

        var result = await sut.GetPartitionsAsync();

        await client.Received(1).GetPartitionsAsync(UserGroupGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }
}
