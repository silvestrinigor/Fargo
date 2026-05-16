using Fargo.Sdk.Contracts.Articles;
using System.Text.Json;

namespace Fargo.HttpApi.Tests.Articles;

public sealed class ArticleInfoTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerOptions.Web);

    [Fact]
    public void Serialize_Should_WriteNestedBarcodes()
    {
        var article = new ArticleInfo(
            Guid.NewGuid(),
            "Article",
            "Description",
            Metrics: null,
            ShelfLife: null,
            new ArticleBarcodesInfo(Ean13: "1234567890123"),
            Partitions: [],
            IsActive: true,
            EditedByGuid: null,
            ArticleModifiedType.None);

        var json = JsonSerializer.Serialize(article, JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("ean13", out _));
        Assert.True(root.TryGetProperty("barcodes", out var barcodes));
        Assert.Equal("1234567890123", barcodes.GetProperty("ean13").GetString());
    }
}
