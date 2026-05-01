using Fargo.Sdk.ApiClients;
using Fargo.Sdk.Http;
using NSubstitute;
using System.Net;

namespace Fargo.Sdk.Tests.ApiClients;

public sealed class ApiClientClientTests
{
    private readonly IFargoHttpClient httpClient = Substitute.For<IFargoHttpClient>();
    private readonly ApiClientHttpClient sut;

    public ApiClientClientTests()
    {
        sut = new ApiClientHttpClient(httpClient);
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var apiClientResult = Fakes.ApiClientResult();
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(true, apiClientResult, null, HttpStatusCode.OK));

        var result = await sut.GetAsync(apiClientResult.Guid);

        Assert.True(result.IsSuccess);
        Assert.Equal(apiClientResult.Guid, result.Data!.Guid);
        Assert.Equal(apiClientResult.Name, result.Data.Name);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsApiClientNotFound()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(false, null, Fakes.Problem("api-client/not-found"), HttpStatusCode.NotFound));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnForbidden_When_ProblemTypeIsUserForbidden()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnUndefined_When_ProblemTypeIsUnknown()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(false, null, Fakes.Problem("server/internal-error"), HttpStatusCode.InternalServerError));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnFallbackDetail_When_ProblemDetailsIsNull()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>(false, null, null, HttpStatusCode.InternalServerError));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
        Assert.Equal("An unexpected error occurred.", result.Error.Detail);
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnApiClients_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<Fargo.Sdk.Contracts.ApiClients.ApiClientDto> clients = [Fakes.ApiClientResult(), Fakes.ApiClientResult()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>>(true, clients, null, HttpStatusCode.OK));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnForbidden_When_AccessDenied()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.ApiClients.ApiClientDto>>(false, null, Fakes.Problem("entity/access-denied"), HttpStatusCode.Forbidden));

        var result = await sut.GetManyAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnCreatedResult_When_HttpResponseIsSuccess()
    {
        var guid = Guid.NewGuid();
        const string plainKey = "test-plain-key";
        httpClient
            .PostFromJsonAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientCreateRequest, ApiClientCreatedDto>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.ApiClients.ApiClientCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<ApiClientCreatedDto>(true, new ApiClientCreatedDto(guid, plainKey), null, HttpStatusCode.Created));

        var result = await sut.CreateAsync("test-client");

        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Data!.Guid);
        Assert.Equal(plainKey, result.Data.PlainKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnForbidden_When_AccessDenied()
    {
        httpClient
            .PostFromJsonAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientCreateRequest, ApiClientCreatedDto>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.ApiClients.ApiClientCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<ApiClientCreatedDto>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.CreateAsync("test-client");

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        httpClient
            .PatchJsonAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientUpdateRequest>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.ApiClients.ApiClientUpdateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.UpdateAsync(Guid.NewGuid(), "New Name", null, null);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNotFound_When_ApiClientDoesNotExist()
    {
        httpClient
            .PatchJsonAsync<Fargo.Sdk.Contracts.ApiClients.ApiClientUpdateRequest>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.ApiClients.ApiClientUpdateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("api-client/not-found"), HttpStatusCode.NotFound));

        var result = await sut.UpdateAsync(Guid.NewGuid(), "New Name", null, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.DeleteAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnForbidden_When_AccessDenied()
    {
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    private static class Fakes
    {
        public static Fargo.Sdk.Contracts.ApiClients.ApiClientDto ApiClientResult() =>
            new(Guid.NewGuid(), "test-client", "A test description", true);

        public static FargoProblemDetails Problem(string type, string detail = "An error occurred.") =>
            new() { Type = type, Detail = detail };
    }
}
