using Fargo.Sdk.Http;
using Fargo.Sdk.Partitions;
using NSubstitute;
using System.Net;

namespace Fargo.Sdk.Tests.Partitions;

public sealed class PartitionClientTests
{
    private readonly IFargoSdkHttpClient httpClient = Substitute.For<IFargoSdkHttpClient>();
    private readonly PartitionClient sut;

    public PartitionClientTests()
    {
        sut = new PartitionClient(httpClient);
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var partitionResult = Fakes.PartitionResult();
        httpClient
            .GetAsync<PartitionResult>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<PartitionResult>(true, partitionResult, null, HttpStatusCode.OK));

        var result = await sut.GetAsync(partitionResult.Guid);

        Assert.True(result.IsSuccess);
        Assert.Equal(partitionResult.Guid, result.Data!.Guid);
        Assert.Equal(partitionResult.Name, result.Data.Name);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsPartitionNotFound()
    {
        httpClient
            .GetAsync<PartitionResult>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<PartitionResult>(false, null, Fakes.Problem("partition/not-found"), HttpStatusCode.NotFound));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnUnauthorizedAccess_When_ProblemTypeIsUnauthorized()
    {
        httpClient
            .GetAsync<PartitionResult>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<PartitionResult>(false, null, Fakes.Problem("auth/unauthorized"), HttpStatusCode.Unauthorized));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.UnauthorizedAccess, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnForbidden_When_ProblemTypeIsUserForbidden()
    {
        httpClient
            .GetAsync<PartitionResult>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<PartitionResult>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnFallbackDetail_When_ProblemDetailsIsNull()
    {
        httpClient
            .GetAsync<PartitionResult>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<PartitionResult>(false, null, null, HttpStatusCode.InternalServerError));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
        Assert.Equal("An unexpected error occurred.", result.Error.Detail);
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnPartitions_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<PartitionResult> partitions = [Fakes.PartitionResult(), Fakes.PartitionResult()];
        httpClient
            .GetAsync<IReadOnlyCollection<PartitionResult>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<PartitionResult>>(true, partitions, null, HttpStatusCode.OK));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<PartitionResult>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<PartitionResult>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnForbidden_When_PartitionAccessDenied()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<PartitionResult>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<PartitionResult>>(false, null, Fakes.Problem("partition/access-denied"), HttpStatusCode.Forbidden));

        var result = await sut.GetManyAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnGuid_When_HttpResponseIsSuccess()
    {
        var guid = Guid.NewGuid();
        httpClient
            .PostFromJsonAsync<object, Guid>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(true, guid, null, HttpStatusCode.Created));

        var result = await sut.CreateAsync("My Partition");

        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Data);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnInvalidInput_When_ProblemTypeIsRequestInvalid()
    {
        httpClient
            .PostFromJsonAsync<object, Guid>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(false, default, Fakes.Problem("request/invalid"), HttpStatusCode.BadRequest));

        var result = await sut.CreateAsync(string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.InvalidInput, result.Error!.Type);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        httpClient
            .PatchJsonAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.UpdateAsync(Guid.NewGuid(), "New Name");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNotFound_When_PartitionDoesNotExist()
    {
        httpClient
            .PatchJsonAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("partition/not-found"), HttpStatusCode.NotFound));

        var result = await sut.UpdateAsync(Guid.NewGuid(), "New Name");

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnInvalidInput_When_CircularHierarchy()
    {
        httpClient
            .PatchJsonAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("partition/circular-hierarchy"), HttpStatusCode.UnprocessableEntity));

        var result = await sut.UpdateAsync(Guid.NewGuid(), null, null, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.InvalidInput, result.Error!.Type);
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
        Assert.Equal(FargoSdkErrorType.InvalidInput, result.Error!.Type);
    }

    private static class Fakes
    {
        public static PartitionResult PartitionResult() =>
            new(Guid.NewGuid(), "Test Partition", "A test partition", null, true);

        public static FargoProblemDetails Problem(string type, string detail = "An error occurred.") =>
            new() { Type = type, Detail = detail };
    }
}
