using Fargo.Sdk.Articles;
using Fargo.Sdk.Events;
using NSubstitute;

namespace Fargo.Sdk.Tests.Articles;

public sealed class ArticleManagerTests
{
    private readonly IArticleClient client = Substitute.For<IArticleClient>();
    private readonly ArticleManager sut;

    public ArticleManagerTests()
    {
        sut = new ArticleManager(client, new FargoHubConnection());
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_Should_ReturnArticle_When_ArticleExists()
    {
        // Arrange
        var result = Fakes.ArticleResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ArticleResult>(result));

        // Act
        var article = await sut.GetAsync(result.Guid);

        // Assert
        Assert.Equal(result.Guid, article.Guid);
        Assert.Equal(result.Name, article.Name);
        Assert.Equal(result.Description, article.Description);
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_ArticleNotFound()
    {
        // Arrange
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ArticleResult>(new FargoSdkError(FargoSdkErrorType.NotFound, "Article not found.")));

        // Act / Assert
        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        // Arrange
        client.GetAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ArticleResult>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        // Act / Assert
        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetAsync(Guid.NewGuid()));
    }

    // --- GetManyAsync ---

    [Fact]
    public async Task GetManyAsync_Should_ReturnArticles_When_ArticlesExist()
    {
        // Arrange
        var results = new[] { Fakes.ArticleResult(), Fakes.ArticleResult() };
        client.GetManyAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ArticleResult>>(results));

        // Act
        var articles = await sut.GetManyAsync();

        // Assert
        Assert.Equal(2, articles.Count);
    }

    [Fact]
    public async Task GetManyAsync_Should_ReturnEmptyCollection_When_NoArticlesExist()
    {
        // Arrange
        client.GetManyAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ArticleResult>>([]));

        // Act
        var articles = await sut.GetManyAsync();

        // Assert
        Assert.Empty(articles);
    }

    [Fact]
    public async Task GetManyAsync_Should_ThrowFargoSdkApiException_When_AccessDenied()
    {
        // Arrange
        client.GetManyAsync(Arg.Any<DateTimeOffset?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<ArticleResult>>(new FargoSdkError(FargoSdkErrorType.Forbidden, "Access denied.")));

        // Act / Assert
        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.GetManyAsync());
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_Should_ReturnArticle_When_CreationSucceeds()
    {
        // Arrange
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<MassDto?>(), Arg.Any<LengthDto?>(), Arg.Any<LengthDto?>(), Arg.Any<LengthDto?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        // Act
        var article = await sut.CreateAsync("My Article", "A description");

        // Assert
        Assert.Equal(guid, article.Guid);
        Assert.Equal("My Article", article.Name);
        Assert.Equal("A description", article.Description);
    }

    [Fact]
    public async Task CreateAsync_Should_UseEmptyDescription_When_DescriptionIsNull()
    {
        // Arrange
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<MassDto?>(), Arg.Any<LengthDto?>(), Arg.Any<LengthDto?>(), Arg.Any<LengthDto?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        // Act
        var article = await sut.CreateAsync("My Article");

        // Assert
        Assert.Equal(string.Empty, article.Description);
    }

    [Fact]
    public async Task CreateAsync_Should_ThrowFargoSdkApiException_When_CreationFails()
    {
        // Arrange
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<MassDto?>(), Arg.Any<LengthDto?>(), Arg.Any<LengthDto?>(), Arg.Any<LengthDto?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Name is required.")));

        // Act / Assert
        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.CreateAsync(string.Empty));
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_Should_CallClient_When_ArticleExists()
    {
        // Arrange
        var guid = Guid.NewGuid();
        client.DeleteAsync(guid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        // Act
        await sut.DeleteAsync(guid);

        // Assert
        await client.Received(1).DeleteAsync(guid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowFargoSdkApiException_When_ArticleHasItems()
    {
        // Arrange
        client.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Article has items.")));

        // Act / Assert
        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.DeleteAsync(Guid.NewGuid()));
    }

    // --- Entity tracking ---

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        // Arrange
        var result = Fakes.ArticleResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ArticleResult>(result));

        var article = await sut.GetAsync(result.Guid);

        ArticleUpdatedEventArgs? received = null;
        article.Updated += (_, e) => received = e;

        // Act
        article.RaiseUpdated();

        // Assert
        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task GetAsync_Should_TrackEntity_So_RaiseDeleted_FiresEntityDeletedEvent()
    {
        // Arrange
        var result = Fakes.ArticleResult();
        client.GetAsync(result.Guid, Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ArticleResult>(result));

        var article = await sut.GetAsync(result.Guid);

        ArticleDeletedEventArgs? received = null;
        article.Deleted += (_, e) => received = e;

        // Act
        article.RaiseDeleted();

        // Assert
        Assert.NotNull(received);
        Assert.Equal(result.Guid, received.Guid);
    }

    [Fact]
    public async Task CreateAsync_Should_TrackEntity_So_RaiseUpdated_FiresEntityUpdatedEvent()
    {
        // Arrange
        var guid = Guid.NewGuid();
        client.CreateAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<MassDto?>(), Arg.Any<LengthDto?>(), Arg.Any<LengthDto?>(), Arg.Any<LengthDto?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<Guid>(guid));

        var article = await sut.CreateAsync("My Article");

        ArticleUpdatedEventArgs? received = null;
        article.Updated += (_, e) => received = e;

        // Act
        article.RaiseUpdated();

        // Assert
        Assert.NotNull(received);
        Assert.Equal(guid, received.Guid);
    }

    private static class Fakes
    {
        public static ArticleResult ArticleResult() =>
            new(Guid.NewGuid(), "Test Article", "A test description");
    }
}
