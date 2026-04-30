using Fargo.Sdk.Articles;
using Fargo.Sdk.Partitions;
using NSubstitute;

namespace Fargo.Sdk.Tests.Articles;

public sealed class ArticleTests
{
    private static readonly Guid ArticleGuid = Guid.NewGuid();
    private readonly IArticleHttpClient client = Substitute.For<IArticleHttpClient>();
    private readonly Article sut;

    public ArticleTests()
    {
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<ArticleMetrics?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>());

        sut = new Article(ArticleGuid, "Original Name", "Original Description", client);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_Should_CallClientUpdateAsync_With_ProvidedValues()
    {
        // Act
        await sut.UpdateAsync(x => x.Name = "New Name");

        // Assert
        await client.Received(1).UpdateAsync(ArticleGuid, "New Name", "Original Description", Arg.Any<ArticleMetrics?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Should_BatchMultipleChangesIntoOneRequest()
    {
        // Act
        await sut.UpdateAsync(x =>
        {
            x.Name = "New Name";
            x.Description = "New Description";
        });

        // Assert
        await client.Received(1).UpdateAsync(ArticleGuid, "New Name", "New Description", Arg.Any<ArticleMetrics?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateLocalValues()
    {
        // Act
        await sut.UpdateAsync(x =>
        {
            x.Name = "New Name";
            x.Description = "New Description";
        });

        // Assert
        Assert.Equal("New Name", sut.Name);
        Assert.Equal("New Description", sut.Description);
    }

    [Fact]
    public async Task UpdateAsync_Should_ThrowFargoSdkApiException_When_UpdateFails()
    {
        // Arrange
        client.UpdateAsync(Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<ArticleMetrics?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<EmptyResult>(new FargoSdkError(FargoSdkErrorType.InvalidInput, "Name is required.")));

        // Act / Assert
        await Assert.ThrowsAsync<FargoSdkApiException>(() => sut.UpdateAsync(x => x.Name = string.Empty));
    }

    // --- Metrics property ---

    [Fact]
    public void Metrics_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        // Arrange
        var metrics = new ArticleMetrics { Mass = new Mass(5, MassUnit.Kilogram) };

        // Act
        sut.Metrics = metrics;

        // Assert
        Assert.Equal(metrics, sut.Metrics);
    }

    // --- ShelfLife property ---

    [Fact]
    public void ShelfLife_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        // Act
        sut.ShelfLife = TimeSpan.FromDays(30);

        // Assert
        Assert.Equal(TimeSpan.FromDays(30), sut.ShelfLife);
    }

    // --- Name / Description getters ---

    [Fact]
    public void Name_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        // Act
        sut.Name = "New Name";

        // Assert
        Assert.Equal("New Name", sut.Name);
    }

    [Fact]
    public void Description_Getter_Should_ReturnUpdatedValue_After_Set()
    {
        // Act
        sut.Description = "New Description";

        // Assert
        Assert.Equal("New Description", sut.Description);
    }

    // --- Guid property ---

    [Fact]
    public void Guid_Should_ReturnGuidPassedAtConstruction()
    {
        Assert.Equal(ArticleGuid, sut.Guid);
    }

    // --- Barcodes property ---

    [Fact]
    public void Barcodes_Should_ReturnTypedBarcodeSlots()
    {
        // Arrange
        var barcodeGuid = Guid.NewGuid();
        var barcodes = new ArticleBarcodes
        {
            Ean13 = new Ean13(barcodeGuid, ArticleGuid, "1234567890123")
        };

        var article = new Article(ArticleGuid, "Name", "Description", client, barcodes: barcodes);

        // Assert
        Assert.Equal("1234567890123", article.Barcodes.Ean13?.Code);
        Assert.Null(article.Barcodes.UpcA);
    }

    [Fact]
    public async Task GetBarcodesAsync_Should_UpdateLocalBarcodeSlots()
    {
        // Arrange
        var barcodeGuid = Guid.NewGuid();
        var barcodes = new ArticleBarcodes
        {
            UpcA = new UpcA(barcodeGuid, ArticleGuid, "123456789012")
        };

        client.GetBarcodesAsync(ArticleGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<ArticleBarcodes>(barcodes));

        // Act
        var result = await sut.GetBarcodesAsync();

        // Assert
        Assert.Equal("123456789012", result.UpcA?.Code);
        Assert.Equal("123456789012", sut.Barcodes.UpcA?.Code);
        Assert.Null(sut.Barcodes.Ean13);
    }

    // --- Updated event ---

    [Fact]
    public void Updated_Should_Fire_When_RaiseUpdatedIsCalled()
    {
        // Arrange
        ArticleUpdatedEventArgs? received = null;
        sut.Updated += (_, e) => received = e;

        // Act
        sut.RaiseUpdated();

        // Assert
        Assert.NotNull(received);
        Assert.Equal(ArticleGuid, received.Guid);
    }

    [Fact]
    public void Updated_Should_NotFire_When_NoHandlerIsAttached()
    {
        // Act / Assert — must not throw
        sut.RaiseUpdated();
    }

    // --- Deleted event ---

    [Fact]
    public void Deleted_Should_Fire_When_RaiseDeletedIsCalled()
    {
        // Arrange
        ArticleDeletedEventArgs? received = null;
        sut.Deleted += (_, e) => received = e;

        // Act
        sut.RaiseDeleted();

        // Assert
        Assert.NotNull(received);
        Assert.Equal(ArticleGuid, received.Guid);
    }

    [Fact]
    public void Deleted_Should_NotFire_When_NoHandlerIsAttached()
    {
        // Act / Assert — must not throw
        sut.RaiseDeleted();
    }

    // --- GetPartitionsAsync ---

    [Fact]
    public async Task GetPartitionsAsync_Should_DelegateToClient()
    {
        // Arrange
        IReadOnlyCollection<PartitionResult> partitions = [new PartitionResult(Guid.NewGuid(), "P1", "desc", null, true)];
        client.GetPartitionsAsync(ArticleGuid, Arg.Any<CancellationToken>())
            .Returns(new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(partitions));

        // Act
        var result = await sut.GetPartitionsAsync();

        // Assert
        await client.Received(1).GetPartitionsAsync(ArticleGuid, Arg.Any<CancellationToken>());
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }
}
