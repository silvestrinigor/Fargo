using Fargo.Application.Articles;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Fargo.Infrastructure.Converters;
using System.Text.Json;

namespace Fargo.HttpApi.Tests.Articles;

public sealed class ArticleDtoTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public void CreateSerialize_Should_WriteArticleType()
    {
        var request = new ArticleCreateDto(
            new Name("Article"),
            ArticleType.Container,
            Container: new ArticleCreateContainerDto());

        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal((int)ArticleType.Container, root.GetProperty("articleType").GetInt32());
    }

    [Fact]
    public void Serialize_Should_WriteNestedBarcodes()
    {
        var article = new ArticleDto(
            Guid.NewGuid(),
            new Name("Article"),
            ArticleType.Default,
            new Description("Description"),
            ShelfLife: null,
            Color: null,
            new ArticleMetricsDto(),
            new ArticleBarcodesDto(Ean13: new Ean13("7891234567895")),
            Partitions: [],
            IsActive: true,
            EditedByGuid: null,
            ArticleModifiedType.None);

        var json = JsonSerializer.Serialize(article, JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.False(root.TryGetProperty("ean13", out _));
        Assert.Equal((int)ArticleType.Default, root.GetProperty("articleType").GetInt32());
        Assert.True(root.TryGetProperty("barcodes", out var barcodes));
        Assert.Equal("7891234567895", barcodes.GetProperty("ean13").GetString());
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Web);
        options.Converters.Add(new NameJsonConverter());
        options.Converters.Add(new DescriptionJsonConverter());
        options.Converters.Add(new Ean13JsonConverter());

        return options;
    }
}
