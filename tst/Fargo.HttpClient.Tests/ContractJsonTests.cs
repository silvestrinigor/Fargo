using Fargo.HttpContracts;
using System.Text.Json;

namespace Fargo.HttpClient.Tests;

public sealed class ContractJsonTests
{
    private static readonly JsonSerializerOptions JsonOptions = FargoHttpJsonSerializerOptions.Create();

    [Fact]
    public void ArticleCreateRequest_Should_SerializeCurrentWireShape()
    {
        var request = new ArticleCreateRequest(
            "Article",
            ArticleType.Container,
            Color: "#ff0000",
            Metrics: new ArticleMetricsDto(Mass: new UnitValueDto(10, "kg")),
            Barcodes: new ArticleBarcodesDto(Ean13: "7891234567895"),
            Container: new ArticleCreateContainerRequest(new UnitValueDto(20, "kg")));

        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("Article", root.GetProperty("name").GetString());
        Assert.Equal((int)ArticleType.Container, root.GetProperty("articleType").GetInt32());
        Assert.Equal("#ff0000", root.GetProperty("color").GetString());
        Assert.Equal("kg", root.GetProperty("metrics").GetProperty("mass").GetProperty("unit").GetString());
        Assert.Equal("7891234567895", root.GetProperty("barcodes").GetProperty("ean13").GetString());
        Assert.Equal(20, root.GetProperty("container").GetProperty("maxMass").GetProperty("value").GetDouble());
    }

    [Fact]
    public void ArticlePatchRequest_Should_OmitUnspecifiedOptionalField()
    {
        var request = new ArticlePatchRequest(Name: "Article");

        var json = JsonSerializer.Serialize(request, JsonOptions);

        Assert.Contains("\"name\":\"Article\"", json);
        Assert.DoesNotContain("shelfLife", json);
    }

    [Fact]
    public void ArticlePatchRequest_Should_WriteExplicitNullableField()
    {
        var request = new ArticlePatchRequest(
            ShelfLife: OptionalField<TimeSpan>.FromValue(null));

        var json = JsonSerializer.Serialize(request, JsonOptions);

        Assert.Contains("\"shelfLife\":null", json);
    }

    [Fact]
    public void ArticlePatchRequest_Should_WriteExplicitTimeSpanField()
    {
        var request = new ArticlePatchRequest(
            ShelfLife: OptionalField<TimeSpan>.FromValue(TimeSpan.FromDays(7)));

        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var document = JsonDocument.Parse(json);

        Assert.Equal("7.00:00:00", document.RootElement.GetProperty("shelfLife").GetString());
    }

    [Fact]
    public void ArticlePatchRequest_Should_ReadOmittedOptionalFieldAsUnspecified()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchRequest>(
            """{"name":"Article"}""",
            JsonOptions);

        Assert.NotNull(request);
        Assert.Equal("Article", request.Name);
        Assert.False(request.ShelfLife.IsSpecified);
    }

    [Fact]
    public void ArticlePatchRequest_Should_ReadExplicitNullOptionalFieldAsSpecifiedNull()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchRequest>(
            """{"shelfLife":null}""",
            JsonOptions);

        Assert.NotNull(request);
        Assert.True(request.ShelfLife.IsSpecified);
        Assert.Null(request.ShelfLife.Value);
    }

    [Fact]
    public void ContractEnums_Should_PreserveCurrentNumericValues()
    {
        Assert.Equal(0, (int)ActionType.CreateArticle);
        Assert.Equal(16, (int)ActionType.EditPartition);
        Assert.Equal(5, (int)ArticleType.Container);
        Assert.Equal(7, (int)BarcodeFormat.Gs1128);
    }
}
