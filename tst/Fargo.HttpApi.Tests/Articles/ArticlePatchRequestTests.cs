using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared;
using Fargo.Infrastructure.Converters;
using System.Text.Json;

namespace Fargo.HttpApi.Tests.Articles;

public sealed class ArticlePatchDtoTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();


    [Fact]
    public void Deserialize_Should_TreatExplicitDescriptionNullAsNoEdit()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchDto>(
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
        var request = JsonSerializer.Deserialize<ArticlePatchDto>(
            """
            {
              "barcodes": {
                "ean13": "7891234567895"
              }
            }
            """,
            JsonOptions)!;

        Assert.NotNull(request.Barcodes);
        Assert.Equal("7891234567895", request.Barcodes.Ean13?.Code);
    }

    [Fact]
    public void Deserialize_Should_ReadName()
    {
        var request = JsonSerializer.Deserialize<ArticlePatchDto>(
            """
            {
              "name": "Article"
            }
            """,
            JsonOptions)!;

        Assert.Equal(new Name("Article"), request.Name);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Web);
        options.Converters.Add(new NameJsonConverter());
        options.Converters.Add(new DescriptionJsonConverter());
        options.Converters.Add(new Ean13JsonConverter());
        options.Converters.Add(new OptionalValueJsonConverterFactory());

        return options;
    }
}
