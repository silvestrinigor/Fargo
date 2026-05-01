using Fargo.Api.Articles;
using Fargo.Api.Http;
using NSubstitute;
using System.Net;

namespace Fargo.Api.Tests.Articles;

public sealed class ArticleClientTests
{
    private readonly IFargoSdkHttpClient httpClient = Substitute.For<IFargoSdkHttpClient>();
    private readonly ArticleClient sut;

    public ArticleClientTests()
    {
        sut = new ArticleClient(httpClient);
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        // Arrange
        var articleResult = Fakes.ArticleResult();
        httpClient
            .GetAsync<Fargo.Api.Contracts.Articles.ArticleDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Articles.ArticleDto>(true, articleResult, null, HttpStatusCode.OK));

        // Act
        var result = await sut.GetAsync(articleResult.Guid);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(articleResult.Guid, result.Data!.Guid);
        Assert.Equal(articleResult.Name, result.Data.Name);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNotFound_When_ProblemTypeIsArticleNotFound()
    {
        // Arrange
        httpClient
            .GetAsync<Fargo.Api.Contracts.Articles.ArticleDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Articles.ArticleDto>(false, null, Fakes.Problem("article/not-found"), HttpStatusCode.NotFound));

        // Act
        var result = await sut.GetAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnUnauthorizedAccess_When_ProblemTypeIsUnauthorized()
    {
        // Arrange
        httpClient
            .GetAsync<Fargo.Api.Contracts.Articles.ArticleDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Articles.ArticleDto>(false, null, Fakes.Problem("auth/unauthorized"), HttpStatusCode.Unauthorized));

        // Act
        var result = await sut.GetAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.UnauthorizedAccess, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnForbidden_When_ProblemTypeIsAccessDenied()
    {
        // Arrange
        httpClient
            .GetAsync<Fargo.Api.Contracts.Articles.ArticleDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Articles.ArticleDto>(false, null, Fakes.Problem("entity/access-denied"), HttpStatusCode.Forbidden));

        // Act
        var result = await sut.GetAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnUndefined_When_ProblemTypeIsUnknown()
    {
        // Arrange
        httpClient
            .GetAsync<Fargo.Api.Contracts.Articles.ArticleDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Articles.ArticleDto>(false, null, Fakes.Problem("server/internal-error"), HttpStatusCode.InternalServerError));

        // Act
        var result = await sut.GetAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnArticles_When_HttpResponseIsSuccess()
    {
        // Arrange
        IReadOnlyCollection<Fargo.Api.Contracts.Articles.ArticleDto> articles = [Fakes.ArticleResult(), Fakes.ArticleResult()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Articles.ArticleDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Articles.ArticleDto>>(true, articles, null, HttpStatusCode.OK));

        // Act
        var result = await sut.GetManyAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        // Arrange
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Articles.ArticleDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Articles.ArticleDto>>(true, null, null, HttpStatusCode.NoContent));

        // Act
        var result = await sut.GetManyAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnForbidden_When_ProblemTypeIsPartitionAccessDenied()
    {
        // Arrange
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Articles.ArticleDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Articles.ArticleDto>>(false, null, Fakes.Problem("partition/access-denied"), HttpStatusCode.Forbidden));

        // Act
        var result = await sut.GetManyAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnGuid_When_HttpResponseIsSuccess()
    {
        // Arrange
        var guid = Guid.NewGuid();
        httpClient
            .PostFromJsonAsync<Fargo.Api.Contracts.Articles.ArticleCreateDto, Guid>(Arg.Any<string>(), Arg.Any<Fargo.Api.Contracts.Articles.ArticleCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(true, guid, null, HttpStatusCode.Created));

        // Act
        var result = await sut.CreateAsync("My Article");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(guid, result.Data);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnInvalidInput_When_ProblemTypeIsRequestInvalid()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<Fargo.Api.Contracts.Articles.ArticleCreateDto, Guid>(Arg.Any<string>(), Arg.Any<Fargo.Api.Contracts.Articles.ArticleCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Guid>(false, default, Fakes.Problem("request/invalid"), HttpStatusCode.BadRequest));

        // Act
        var result = await sut.CreateAsync(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.InvalidInput, result.Error!.Type);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        // Arrange
        httpClient
            .PatchJsonAsync<Fargo.Api.Contracts.Articles.ArticleUpdateDto>(Arg.Any<string>(), Arg.Any<Fargo.Api.Contracts.Articles.ArticleUpdateDto>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), "New Name");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        // Arrange
        httpClient
            .PatchJsonAsync<Fargo.Api.Contracts.Articles.ArticleUpdateDto>(Arg.Any<string>(), Arg.Any<Fargo.Api.Contracts.Articles.ArticleUpdateDto>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("article/not-found"), HttpStatusCode.NotFound));

        // Act
        var result = await sut.UpdateAsync(Guid.NewGuid(), "New Name");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        // Arrange
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnInvalidInput_When_ArticleHasItems()
    {
        // Arrange
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("article/delete-with-items"), HttpStatusCode.Conflict));

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.InvalidInput, result.Error!.Type);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnForbidden_When_ProblemTypeIsUserForbidden()
    {
        // Arrange
        httpClient
            .DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("user/forbidden"), HttpStatusCode.Forbidden));

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Forbidden, result.Error!.Type);
    }

    // --- GetPartitionsAsync ---

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnPartitions_When_HttpResponseIsSuccess()
    {
        // Arrange
        IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto> partitions = [Fakes.PartitionResult()];
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(true, partitions, null, HttpStatusCode.OK));

        // Act
        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnEmptyCollection_When_DataIsNull()
    {
        // Arrange
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(true, null, null, HttpStatusCode.NoContent));

        // Act
        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetPartitionsAsync_Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        // Arrange
        httpClient
            .GetAsync<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<IReadOnlyCollection<Fargo.Api.Contracts.Partitions.PartitionDto>>(false, null, Fakes.Problem("article/not-found"), HttpStatusCode.NotFound));

        // Act
        var result = await sut.GetPartitionsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnFallbackDetail_When_ProblemDetailsIsNull()
    {
        // Arrange
        httpClient
            .GetAsync<Fargo.Api.Contracts.Articles.ArticleDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<Fargo.Api.Contracts.Articles.ArticleDto>(false, null, null, HttpStatusCode.InternalServerError));

        // Act
        var result = await sut.GetAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
        Assert.Equal("An unexpected error occurred.", result.Error.Detail);
    }

    private static class Fakes
    {
        public static Fargo.Api.Contracts.Articles.ArticleDto ArticleResult() =>
            new(Guid.NewGuid(), "Test Article", "A test description");

        public static Fargo.Api.Contracts.Partitions.PartitionDto PartitionResult() =>
            new(Guid.NewGuid(), "Test Partition", "A test partition", null, true);

        public static FargoProblemDetails Problem(string type, string detail = "An error occurred.") =>
            new() { Type = type, Detail = detail };
    }
}
