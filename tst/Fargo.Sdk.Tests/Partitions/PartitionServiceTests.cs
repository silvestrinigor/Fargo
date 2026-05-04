using Fargo.Api.Partitions;
using NSubstitute;

namespace Fargo.Api.Tests.Partitions;

public sealed class PartitionServiceTests
{
    private readonly IPartitionHttpClient client = Substitute.For<IPartitionHttpClient>();
    private readonly PartitionService sut;

    public PartitionServiceTests()
    {
        sut = new PartitionService(client, Substitute.For<IFargoEventHub>());
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnPartition_When_PartitionExists()
    {
        var result = Fakes.PartitionResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<PartitionResult>(result));

        var partition = await sut.GetAsync(result.Guid);

        Assert.Equal(result.Guid, partition.Guid);
        Assert.Equal(result.Name, partition.Name);
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_PartitionNotFound()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<PartitionResult>(new FargoSdkError(FargoSdkErrorType.NotFound, "Partition not found.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<PartitionResult>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnPartitions_When_PartitionsExist()
    {
        var results = new[] { Fakes.PartitionResult(), Fakes.PartitionResult() };
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(results));

        var partitions = await sut.GetManyAsync();

        Assert.Equal(2, partitions.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_NoPartitionsExist()
    {
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>([]));

        var partitions = await sut.GetManyAsync();

        Assert.Empty(partitions);
    }

    [Fact]
    public async Task GetManyAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<bool?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetManyAsync());
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnPartition_When_CreationSucceeds()
    {
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var partition = await sut.CreateAsync("My Partition", "A description");

        Assert.Equal(guid, partition.Guid);
        Assert.Equal("My Partition", partition.Name);
        Assert.Equal("A description", partition.Description);
    }

    [Fact]
    public async Task CreateAsync_Should_UseEmptyDescription_When_DescriptionIsNull()
    {
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var partition = await sut.CreateAsync("My Partition");

        Assert.Equal(string.Empty, partition.Description);
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowFargoSdkApiException_When_CreationFails()
    {
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Name is required.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.CreateAsync(string.Empty));
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_CallClient_When_PartitionExists()
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
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Partition has children.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.DeleteAsync(Guid.NewGuid()));
    }

    // --- Entity tracking ---

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var result = Fakes.PartitionResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<PartitionResult>(result));

        var partition = await sut.GetAsync(result.Guid);

        PartitionUpdatedEventArgs? received = null;
        partition.Updated += (_, e) => received = e;

        partition.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseDeleted_FiresEntityDeletedEvent()
    {
        var result = Fakes.PartitionResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<PartitionResult>(result));

        var partition = await sut.GetAsync(result.Guid);

        PartitionDeletedEventArgs? received = null;
        partition.Deleted += (_, e) => received = e;

        partition.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task CreateAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var partition = await sut.CreateAsync("My Partition");

        PartitionUpdatedEventArgs? received = null;
        partition.Updated += (_, e) => received = e;

        partition.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(guid, received.Guid);
    }

    private static class Fakes
    {
        public static PartitionResult PartitionResult() =>
            new(Guid.NewGuid(), "Test Partition", "A test description", null, true);
    }
}
