using Fargo.Sdk.Http;
using Fargo.Sdk.Items;
using NSubstitute;
using System.Net;

namespace Fargo.Sdk.Tests.Items;

public sealed class ItemClientTests
{
    private readonly IFargoHttpClient httpClient = Substitute.For<IFargoHttpClient>();
    private readonly ItemHttpClient sut;

    public ItemClientTests()
    {
        sut = new ItemHttpClient(httpClient);
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var itemResult = Fakes.ItemInfo();
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Items.ItemInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Items.ItemInfo>(true, itemResult, null, HttpStatusCode.OK));

        var result = await sut.GetAsync(itemResult.Guid);

        Assert.True(result.IsSuccess);
        Assert.Equal(itemResult.Guid, result.Data!.Guid);
        Assert.Equal(itemResult.ArticleGuid, result.Data.ArticleGuid);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsItemNotFound()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Items.ItemInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Items.ItemInfo>(false, null, Fakes.Problem("item/not-found"), HttpStatusCode.NotFound));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsArticleNotFound()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Items.ItemInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Items.ItemInfo>(false, null, Fakes.Problem("article/not-found"), HttpStatusCode.NotFound));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnUnauthorizedAccess_When_ProblemTypeIsUnauthorized()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Items.ItemInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Items.ItemInfo>(false, null, Fakes.Problem("auth/unauthorized"), HttpStatusCode.Unauthorized));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.UnauthorizedAccess, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnForbidden_When_ProblemTypeIsUserForbidden()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Items.ItemInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Items.ItemInfo>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnFallbackDetail_When_ProblemDetailsIsNull()
    {
        httpClient
            .GetAsync<Fargo.Sdk.Contracts.Items.ItemInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Sdk.Contracts.Items.ItemInfo>(false, null, null, HttpStatusCode.InternalServerError));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
        Assert.Equal("An unexpected error occurred.", result.Error.Detail);
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnItems_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo> items = [Fakes.ItemInfo(), Fakes.ItemInfo()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo>>(true, items, null, HttpStatusCode.OK));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnForbidden_When_PartitionAccessDenied()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Items.ItemInfo>>(false, null, Fakes.Problem("partition/access-denied"), HttpStatusCode.Forbidden));

        var result = await sut.GetManyAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnGuid_When_HttpResponseIsSuccess()
    {
        var guid = Guid.NewGuid();
        var request = new Fargo.Sdk.Contracts.Items.ItemCreateRequest(Guid.NewGuid());
        httpClient
            .PostFromJsonAsync<Fargo.Sdk.Contracts.Items.ItemCreateRequest, Guid>(Arg.Any<string>(), Arg.Is(request), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(true, guid, null, HttpStatusCode.Created));

        var result = await sut.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Data);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        httpClient
            .PostFromJsonAsync<Fargo.Sdk.Contracts.Items.ItemCreateRequest, Guid>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.Items.ItemCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(false, default, Fakes.Problem("article/not-found"), HttpStatusCode.NotFound));

        var result = await sut.CreateAsync(new Fargo.Sdk.Contracts.Items.ItemCreateRequest(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var itemGuid = Guid.NewGuid();
        var partitionGuid = Guid.NewGuid();
        var parentContainerGuid = Guid.NewGuid();

        httpClient
            .PutJsonAsync(
                $"/items/{itemGuid}",
                Arg.Is<Fargo.Sdk.Contracts.Items.ItemUpdateRequest>(dto =>
                    dto.Partitions.SequenceEqual(new[] { partitionGuid }) &&
                    dto.ParentContainerGuid == parentContainerGuid),
                Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.UpdateAsync(itemGuid, new Fargo.Sdk.Contracts.Items.ItemUpdateRequest([partitionGuid], parentContainerGuid));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnInvalidInput_When_CircularContainerHierarchy()
    {
        httpClient
            .PutJsonAsync<Fargo.Sdk.Contracts.Items.ItemUpdateRequest>(Arg.Any<string>(), Arg.Any<Fargo.Sdk.Contracts.Items.ItemUpdateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("item/circular-container-hierarchy"), HttpStatusCode.BadRequest));

        var result = await sut.UpdateAsync(Guid.NewGuid(), new Fargo.Sdk.Contracts.Items.ItemUpdateRequest([], Guid.NewGuid()));

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
    public async Task DeleteAsync_Should_ReturnForbidden_When_AccessDenied()
    {
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.DeleteAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    // --- GetPartitionsAsync ---

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnPartitions_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo> partitions = [Fakes.PartitionInfo()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(true, partitions, null, HttpStatusCode.OK));

        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Sdk.Contracts.Partitions.PartitionInfo>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    private static class Fakes
    {
        public static Fargo.Sdk.Contracts.Items.ItemInfo ItemInfo() => new(Guid.NewGuid(), Guid.NewGuid());

        public static Fargo.Sdk.Contracts.Partitions.PartitionInfo PartitionInfo() =>
            new(Guid.NewGuid(), "Test Partition", "A test partition", null, true);

        public static FargoProblemDetails Problem(string type, string detail = "An error occurred.") =>
            new() { Type = type, Detail = detail };
    }
}
