using Fargo.Sdk.Contracts;
using Fargo.Sdk.Contracts.Articles;
using System.Text.Json;

namespace Fargo.HttpApi.Tests.Articles;

public sealed class ArticlePatchRequestTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerOptions.Web);

    [Fact]
    public void Deserialize_Should_KeepOmittedFieldUnspecified()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchRequest>("{}", JsonOptions)!;

        Assert.Null(request.Name);
        Assert.False(request.ShelfLife.IsSpecified);
        Assert.Null(request.Barcodes);
    }

    [Fact]
    public void Deserialize_Should_KeepExplicitNullableDatabaseFieldSpecified()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchRequest>(
            """
            {
              "shelfLife": null
            }
            """,
            JsonOptions)!;

        Assert.True(request.ShelfLife.IsSpecified);
        Assert.Null(request.ShelfLife.Value);
    }

    [Fact]
    public void Deserialize_Should_TreatExplicitDescriptionNullAsNoEdit()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchRequest>(
            """
            {
              "description": null
            }
            """,
            JsonOptions)!;

        Assert.Null(request.Description);
    }

    [Fact]
    public void Deserialize_Should_ReadNestedBarcodes()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchRequest>(
            """
            {
              "barcodes": {
                "ean13": "1234567890123"
              }
            }
            """,
            JsonOptions)!;

        Assert.NotNull(request.Barcodes);
        Assert.Equal("1234567890123", request.Barcodes.Ean13);
    }

    [Fact]
    public void Serialize_Should_OmitUnspecifiedFields()
    {
        var request = new ArticlePatchRequest
        {
            Name = "Article"
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);

        Assert.Contains("\"name\":\"Article\"", json);
        Assert.DoesNotContain("description", json);
    }
}
