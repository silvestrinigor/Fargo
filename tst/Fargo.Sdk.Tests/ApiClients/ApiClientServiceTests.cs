using Fargo.Sdk.ApiClients;
using Fargo.Sdk.Events;
using NSubstitute;

namespace Fargo.Sdk.Tests.ApiClients;

public sealed class ApiClientServiceTests
{
    private readonly IApiClientHttpClient client = Substitute.For<IApiClientHttpClient>();
    private readonly ApiClientService sut;

    public ApiClientServiceTests()
    {
        sut = new ApiClientService(client, Substitute.For<IFargoEventHub>());
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnApiClient_When_ApiClientExists()
    {
        var result = Fakes.ApiClientResult();
        client.GetAsync(result.Guid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ApiClientResult>(result));

        var apiClient = await sut.GetAsync(result.Guid);

        Assert.Equal(result.Guid, apiClient.Guid);
        Assert.Equal(result.Name, apiClient.Name);
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_ApiClientNotFound()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ApiClientResult>(new FargoSdkError(FargoSdkErrorType.NotFound, "API client not found.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ApiClientResult>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnApiClients_When_ApiClientsExist()
    {
        var results = new[] { Fakes.ApiClientResult(), Fakes.ApiClientResult() };
        client.GetManyAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ApiClientResult>>(results));

        var apiClients = await sut.GetManyAsync();

        Assert.Equal(2, apiClients.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_NoApiClientsExist()
    {
        client.GetManyAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ApiClientResult>>([]));

        var apiClients = await sut.GetManyAsync();

        Assert.Empty(apiClients);
    }

    [Fact]
    public async Task GetManyAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        client.GetManyAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ApiClientResult>>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetManyAsync());
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnClientAndPlainKey_When_CreationSucceeds()
    {
        var guid = Guid.NewGuid();
        const string plainKey = "test-plain-key";
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ApiClientCreatedResult>(new ApiClientCreatedResult(guid, plainKey)));

        var (apiClient, key) = await sut.CreateAsync("test-client", "A description");

        Assert.Equal(guid, apiClient.Guid);
        Assert.Equal("test-client", apiClient.Name);
        Assert.Equal(plainKey, key);
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowFargoSdkApiException_When_CreationFails()
    {
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ApiClientCreatedResult>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Name is required.")));

        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.CreateAsync(string.Empty));
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_CallClient_When_ApiClientExists()
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
        var result = Fakes.ApiClientResult();
        client.GetAsync(result.Guid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ApiClientResult>(result));

        var apiClient = await sut.GetAsync(result.Guid);

        ApiClientUpdatedEventArgs? received = null;
        apiClient.Updated += (_, e) => received = e;

        apiClient.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseDeleted_FiresEntityDeletedEvent()
    {
        var result = Fakes.ApiClientResult();
        client.GetAsync(result.Guid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ApiClientResult>(result));

        var apiClient = await sut.GetAsync(result.Guid);

        ApiClientDeletedEventArgs? received = null;
        apiClient.Deleted += (_, e) => received = e;

        apiClient.RaiseDeleted();

        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task CreateAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ApiClientCreatedResult>(new ApiClientCreatedResult(guid, "key")));

        var (apiClient, _) = await sut.CreateAsync("test-client");

        ApiClientUpdatedEventArgs? received = null;
        apiClient.Updated += (_, e) => received = e;

        apiClient.RaiseUpdated();

        Assert.NotNull(received);
        Assert.Equal(guid, received.Guid);
    }

    private static class Fakes
    {
        public static ApiClientResult ApiClientResult() =>
            new(Guid.NewGuid(), "test-client", "A test description", true);
    }
}
