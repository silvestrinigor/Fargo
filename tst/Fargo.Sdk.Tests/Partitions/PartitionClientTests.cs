using Fargo.Sdk.Contracts.Errors;
using Fargo.Sdk.Http;
using Fargo.Sdk.Partitions;
using NSubstitute;
using System.Net;

namespace Fargo.Sdk.Tests.Partitions;

public sealed class PartitionClientTests
{
    private readonly IFargoHttpClient httpClient = Substitute.For<IFargoHttpClient>();
    private readonly PartitionHttpClient sut;

    public PartitionClientTests()
    {
        sut = new PartitionHttpClient(httpClient);
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var partitionResult = Fakes.PartitionInfo();
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(true, partitionResult, null, HttpStatusCode.OK));

        var result = await sut.GetAsync(partitionResult.Guid);

        Assert.True(result.IsSuccess);
        Assert.Equal(partitionResult.Guid, result.Result!.Guid);
        Assert.Equal(partitionResult.Name, result.Result.Name);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsPartitionNotFound()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(false, null, Fakes.Problem("partition/not-found"), HttpStatusCode.NotFound));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnUnauthorizedAccess_When_ProblemTypeIsUnauthorized()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(false, null, Fakes.Problem("auth/unauthorized"), HttpStatusCode.Unauthorized));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnForbidden_When_ProblemTypeIsUserForbidden()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnFallbackDetail_When_ProblemDetailsIsNull()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Partitions.PartitionInfo>(false, null, null, HttpStatusCode.InternalServerError));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Null(result.Error!.Type);
        Assert.Equal("An unexpected error occurred.", result.Error.Detail);
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnPartitions_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo> partitions = [Fakes.PartitionInfo(), Fakes.PartitionInfo()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(true, partitions, null, HttpStatusCode.OK));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Result!.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Result!);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnForbidden_When_PartitionAccessDenied()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(false, null, Fakes.Problem("partition/access-denied"), HttpStatusCode.Forbidden));

        var result = await sut.GetManyAsync();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.Type);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnGuid_When_HttpResponseIsSuccess()
    {
        var guid = Guid.NewGuid();
        var request = new Fargo.Sdk.Contracts.Partitions.PartitionCreateRequest("My Partition");
        httpClient
            .PostFromJsonAsync<Fargo.Sdk.Contracts.Partitions.PartitionCreateRequest, Guid>(Arg.Any<string>(), Arg.Is(request), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(true, guid, null, HttpStatusCode.Created));

        var result = await sut.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Result);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnInvalidInput_When_ProblemTypeIsRequestInvalid()
    {
        httpClient
            .PostFromJsonAsync<Fargo.Sdk.Contracts.Partitions.PartitionCreateRequest, Guid>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.Partitions.PartitionCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(false, default, Fakes.Problem("request/invalid"), HttpStatusCode.BadRequest));

        var result = await sut.CreateAsync(new Fargo.Sdk.Contracts.Partitions.PartitionCreateRequest(string.Empty));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.Type);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var request = new Fargo.Sdk.Contracts.Partitions.PartitionUpdateRequest("New Name");
        httpClient
            .PutJsonAsync<Fargo.Sdk.Contracts.Partitions.PartitionUpdateRequest>(Arg.Any<string>(), Arg.Is(request), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.UpdateAsync(Guid.NewGuid(), request);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNotFound_When_PartitionDoesNotExist()
    {
        httpClient
            .PutJsonAsync<Fargo.Sdk.Contracts.Partitions.PartitionUpdateRequest>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.Partitions.PartitionUpdateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("partition/not-found"), HttpStatusCode.NotFound));

        var result = await sut.UpdateAsync(Guid.NewGuid(), new Fargo.Sdk.Contracts.Partitions.PartitionUpdateRequest("New Name"));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.Type);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnInvalidInput_When_CircularHierarchy()
    {
        httpClient
            .PutJsonAsync<Fargo.Sdk.Contracts.Partitions.PartitionUpdateRequest>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.Partitions.PartitionUpdateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("partition/circular-hierarchy"), HttpStatusCode.UnprocessableEntity));

        var result = await sut.UpdateAsync(Guid.NewGuid(), new Fargo.Sdk.Contracts.Partitions.PartitionUpdateRequest(ParentPartitionGuid: Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.Type);
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
    public async Task DeleteAsync_Should_ReturnInvalidInput_When_CannotDeleteGlobalPartition()
    {
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("partition/cannot-delete-global"), HttpStatusCode.Conflict));

        var result = await sut.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error!.Type);
    }

    private static class Fakes
    {
        public static Fargo.Sdk.Contracts.Partitions.PartitionInfo PartitionInfo() =>
            new(Guid.NewGuid(), "Test Partition", "A test partition", null, true);

        public static FargoProblemDetails Problem(string type, string detail = "An error occurred.") =>
            new() { Type = type, Detail = detail };
    }
}
