using Fargo.Sdk.Partitions;
using NSubstitute;

namespace Fargo.Sdk.Tests.Partitions;

public sealed class PartitionTests
{
    private static readonly Guid PartitionGuid = Guid.NewGuid();
    private readonly IPartitionHttpClient client = Substitute.For<IPartitionHttpClient>();
    private readonly Partition sut;

    public PartitionTests()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        sut = new Partition(PartitionGuid, "Original Name", "Original Description", null, true, client);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_CallClientUpdateAsync_With_ProvidedValues()
    {
        await sut.UpdateAsync(x => x.Name = "New Name");

        await client.Received(1).UpdateAsync(PartitionGuid, "New Name", Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateLocalValues()
    {
        await sut.UpdateAsync(x =>
        {
            x.Name = "New Name";
            x.Description = "New Description";
        });

        Assert.Equal("New Name", sut.Name);
        Assert.Equal("New Description", sut.Description);
    }

    [Fact]
    public async Task UpdateAsync_Should_ThrowFargoSdkApiException_When_UpdateFails()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Name is required.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.UpdateAsync(x => x.Name = string.Empty));
    }

    // --- MoveAsync ---

    [Fact]
    public async Task MoveAsync_Should_DelegateToClient()
    {
        var newParentGuid = Guid.NewGuid();
        client.UpdateAsync(PartitionGuid, null, null, newParentGuid, null, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        var result = await sut.MoveAsync(newParentGuid);

        await client.Received(1).UpdateAsync(PartitionGuid, null, null, newParentGuid, null, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
    }

    // --- Property getters ---

    [Fact]
    public void Name_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.Name = "New Name";

        Assert.Equal("New Name", sut.Name);
    }

    [Fact]
    public void Description_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        sut.Description = "New Description";

        Assert.Equal("New Description", sut.Description);
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
        Assert.Equal(PartitionGuid, sut.Guid);
    }

    // --- Updated event ---

    [Fact]
    public void Updated_Should_Fire_When_RaiseUpdatedIsCalled()
    {
        PartitionUpdatedEventArgs? received = null;
        sut.Updated += (_, e) => received = e;

        sut.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(PartitionGuid, received.Guid);
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
        PartitionDeletedEventArgs? received = null;
        sut.Deleted += (_, e) => received = e;

        sut.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(PartitionGuid, received.Guid);
    }

    [Fact]
    public void Deleted_Should_NotFire_When_NoHandlerIsAttached()
    {
        sut.RaiseDeleted();
    }
}
