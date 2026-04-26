using Fargo.Sdk.Items;
using Fargo.Sdk.Partitions;
using NSubstitute;

namespace Fargo.Sdk.Tests.Items;

public sealed class ItemTests
{
    private static readonly Guid ItemGuid = Guid.NewGuid();
    private static readonly Guid ArticleGuid = Guid.NewGuid();
    private readonly IItemHttpClient client = Substitute.For<IItemHttpClient>();
    private readonly Item sut;

    public ItemTests()
    {
        sut = new Item(ItemGuid, ArticleGuid, client);
    }

    // --- Property getters ---

    [Fact]
    public void Guid_Should_ReturnGuidPassedAtConstruction()
    {
        Assert.Equal(ItemGuid, sut.Guid);
    }

    [Fact]
    public void ArticleGuid_Should_ReturnArticleGuidPassedAtConstruction()
    {
        Assert.Equal(ArticleGuid, sut.ArticleGuid);
    }

    // --- Updated event ---

    [Fact]
    public void Updated_Should_Fire_When_RaiseUpdatedIsCalled()
    {
        ItemUpdatedEventArgs? received = null;
        sut.Updated += (_, e) => received = e;

        sut.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(ItemGuid, received.Guid);
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
        ItemDeletedEventArgs? received = null;
        sut.Deleted += (_, e) => received = e;

        sut.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(ItemGuid, received.Guid);
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
        client.GetPartitionsAsync(ItemGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(partitions));

        var result = await sut.GetPartitionsAsync();

        await client.Received(1).GetPartitionsAsync(ItemGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    // --- AddPartitionAsync / RemovePartitionAsync ---

    [Fact]
    public async Task AddPartitionAsync_Should_DelegateToClient()
    {
        var partitionGuid = Guid.NewGuid();
        client.AddPartitionAsync(ItemGuid, partitionGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        var result = await sut.AddPartitionAsync(partitionGuid);

        await client.Received(1).AddPartitionAsync(ItemGuid, partitionGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RemovePartitionAsync_Should_DelegateToClient()
    {
        var partitionGuid = Guid.NewGuid();
        client.RemovePartitionAsync(ItemGuid, partitionGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        var result = await sut.RemovePartitionAsync(partitionGuid);

        await client.Received(1).RemovePartitionAsync(ItemGuid, partitionGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
    }
}
