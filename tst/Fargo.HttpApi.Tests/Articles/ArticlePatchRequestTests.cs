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

        Assert.False(request.Name.IsSpecified);
    }

    [Fact]
    public void Deserialize_Should_KeepExplicitNullSpecified()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchRequest>(
            """
            {
              "ean13": null
            }
            """,
            JsonOptions)!;

        Assert.True(request.Ean13.IsSpecified);
        Assert.Null(request.Ean13.Value);
    }

    [Fact]
    public void Serialize_Should_OmitUnspecifiedFields()
    {
        var request = new ArticlePatchRequest
        {
            Name = Optional<string?>.FromValue("Article")
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);

        Assert.Contains("\"name\":\"Article\"", json);
        Assert.DoesNotContain("description", json);
    }
}
