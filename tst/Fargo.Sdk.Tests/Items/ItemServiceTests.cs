using Fargo.Api.Items;
using NSubstitute;

namespace Fargo.Api.Tests.Items;

public sealed class ItemServiceTests
{
    private readonly IItemHttpClient client = Substitute.For<IItemHttpClient>();
    private readonly ItemService sut;

    public ItemServiceTests()
    {
        sut = new ItemService(client, Substitute.For<IFargoEventHub>());
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnItem_When_ItemExists()
    {
        var result = Fakes.ItemResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ItemResult>(result));

        var item = await sut.GetAsync(result.Guid);

        Assert.Equal(result.Guid, item.Guid);
        Assert.Equal(result.ArticleGuid, item.ArticleGuid);
    }

    [Fact]
    public async Task GetAsync_Should_MapProductionDate_When_ProductionDateIsPresent()
    {
        var productionDate = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var result = Fakes.ItemResultWithProductionDate(productionDate);
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ItemResult>(result));

        var item = await sut.GetAsync(result.Guid);

        Assert.Equal(productionDate, item.ProductionDate);
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_ItemNotFound()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ItemResult>(new FargoSdkError(FargoSdkErrorType.NotFound, "Item not found.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ItemResult>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnItems_When_ItemsExist()
    {
        var results = new[] { Fakes.ItemResult(), Fakes.ItemResult() };
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ItemResult>>(results));

        var items = await sut.GetManyAsync();

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_NoItemsExist()
    {
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ItemResult>>([]));

        var items = await sut.GetManyAsync();

        Assert.Empty(items);
    }

    [Fact]
    public async Task GetManyAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetManyAsync(Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ItemResult>>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetManyAsync());
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnItem_When_CreationSucceeds()
    {
        var guid = Guid.NewGuid();
        var articleGuid = Guid.NewGuid();
        client.CreateAsync(articleGuid, Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var item = await sut.CreateAsync(articleGuid);

        Assert.Equal(guid, item.Guid);
        Assert.Equal(articleGuid, item.ArticleGuid);
    }

    [Fact]
    public async Task CreateAsync_Should_PassProductionDate_When_Provided()
    {
        var guid = Guid.NewGuid();
        var articleGuid = Guid.NewGuid();
        var productionDate = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        client.CreateAsync(articleGuid, Arg.Any<Guid?>(), productionDate, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var item = await sut.CreateAsync(articleGuid, productionDate: productionDate);

        Assert.Equal(productionDate, item.ProductionDate);
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowFargoSdkApiException_When_CreationFails()
    {
        client.CreateAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(new FargoSdkError(FargoSdkErrorType.NotFound, "Article not found.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.CreateAsync(Guid.NewGuid()));
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_CallClient_When_ItemExists()
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
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.DeleteAsync(Guid.NewGuid()));
    }

    // --- Entity tracking ---

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var result = Fakes.ItemResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ItemResult>(result));

        var item = await sut.GetAsync(result.Guid);

        ItemUpdatedEventArgs? received = null;
        item.Updated += (_, e) => received = e;

        item.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseDeleted_FiresEntityDeletedEvent()
    {
        var result = Fakes.ItemResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ItemResult>(result));

        var item = await sut.GetAsync(result.Guid);

        ItemDeletedEventArgs? received = null;
        item.Deleted += (_, e) => received = e;

        item.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task CreateAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var guid = Guid.NewGuid();
        var articleGuid = Guid.NewGuid();
        client.CreateAsync(articleGuid, Arg.Any<Guid?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var item = await sut.CreateAsync(articleGuid);

        ItemUpdatedEventArgs? received = null;
        item.Updated += (_, e) => received = e;

        item.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(guid, received.Guid);
    }

    private static class Fakes
    {
        public static ItemResult ItemResult() => new(Guid.NewGuid(), Guid.NewGuid());

        public static ItemResult ItemResultWithProductionDate(DateTimeOffset productionDate) =>
            new(Guid.NewGuid(), Guid.NewGuid(), productionDate);
    }
}
