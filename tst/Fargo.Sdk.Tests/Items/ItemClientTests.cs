using Fargo.Api.Http;
using Fargo.Api.Items;
using NSubstitute;
using System.Net;

namespace Fargo.Api.Tests.Items;

public sealed class ItemClientTests
{
    private readonly IFargoSdkHttpClient httpClient = Substitute.For<IFargoSdkHttpClient>();
    private readonly ItemClient sut;

    public ItemClientTests()
    {
        sut = new ItemClient(httpClient);
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        var itemResult = Fakes.ItemResult();
        httpClient
            .GetAsync<Fargo.Api.Contracts.Items.ItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Items.ItemDto>(true, itemResult, null, HttpStatusCode.OK));

        var result = await sut.GetAsync(itemResult.Guid);

        Assert.True(result.IsSuccess);
        Assert.Equal(itemResult.Guid, result.Data!.Guid);
        Assert.Equal(itemResult.ArticleGuid, result.Data.ArticleGuid);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsItemNotFound()
    {
        httpClient
            .GetAsync<Fargo.Api.Contracts.Items.ItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Items.ItemDto>(false, null, Fakes.Problem("item/not-found"), HttpStatusCode.NotFound));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsArticleNotFound()
    {
        httpClient
            .GetAsync<Fargo.Api.Contracts.Items.ItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Items.ItemDto>(false, null, Fakes.Problem("article/not-found"), HttpStatusCode.NotFound));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnUnauthorizedAccess_When_ProblemTypeIsUnauthorized()
    {
        httpClient
            .GetAsync<Fargo.Api.Contracts.Items.ItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Items.ItemDto>(false, null, Fakes.Problem("auth/unauthorized"), HttpStatusCode.Unauthorized));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.UnauthorizedAccess, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnForbidden_When_ProblemTypeIsUserForbidden()
    {
        httpClient
            .GetAsync<Fargo.Api.Contracts.Items.ItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Items.ItemDto>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnFallbackDetail_When_ProblemDetailsIsNull()
    {
        httpClient
            .GetAsync<Fargo.Api.Contracts.Items.ItemDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Items.ItemDto>(false, null, null, HttpStatusCode.InternalServerError));

        var result = await sut.GetAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
        Assert.Equal("An unexpected error occurred.", result.Error.Detail);
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnItems_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<Fargo.Api.Contracts.Items.ItemDto> items = [Fakes.ItemResult(), Fakes.ItemResult()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Items.ItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Items.ItemDto>>(true, items, null, HttpStatusCode.OK));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Items.ItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Items.ItemDto>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetManyAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnForbidden_When_PartitionAccessDenied()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Items.ItemDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Items.ItemDto>>(false, null, Fakes.Problem("partition/access-denied"), HttpStatusCode.Forbidden));

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
            .PostFromJsonAsync<Fargo.Api.Contracts.Items.ItemCreateDto, Guid>(Arg.Any<string>(), Arg.Any<Fargo.Api.Contracts.Items.ItemCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(true, guid, null, HttpStatusCode.Created));

        var result = await sut.CreateAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Data);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        httpClient
            .PostFromJsonAsync<Fargo.Api.Contracts.Items.ItemCreateDto, Guid>(Arg.Any<string>(), Arg.Any<Fargo.Api.Contracts.Items.ItemCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(false, default, Fakes.Problem("article/not-found"), HttpStatusCode.NotFound));

        var result = await sut.CreateAsync(Guid.NewGuid());

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

    // --- GetPartitionsAsync ---

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnPartitions_When_HttpResponseIsSuccess()
    {
        IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto> partitions = [Fakes.PartitionResult()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(true, partitions, null, HttpStatusCode.OK));

        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(true, null, null, HttpStatusCode.NoContent));

        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    private static class Fakes
    {
        public static Fargo.Api.Contracts.Items.ItemDto ItemResult() => new(Guid.NewGuid(), Guid.NewGuid());

        public static Fargo.Api.Contracts.Partitions.PartitionDto PartitionResult() =>
            new(Guid.NewGuid(), "Test Partition", "A test partition", null, true);

        public static FargoProblemDetails Problem(string type, string detail = "An error occurred.") =>
            new() { Type = type, Detail = detail };
    }
}
