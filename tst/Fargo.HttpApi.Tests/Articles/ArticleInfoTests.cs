using Fargo.Application.Articles;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Fargo.HttpApi.Articles;
using Fargo.Infrastructure.Converters;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

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
    public void CreateVariationCommand_Should_RequireVariationPayload()
    {
        var request = new ArticleCreateDto(
            new Name("Variation"),
            ArticleType.Variation);

        Assert.Throws<ArgumentException>(
            () => ArticleEndpointRouteBuilderExtension.CreateVariationCommand(request));
    }

    [Fact]
    public void CreatePackCommand_Should_MapTypeSpecificInputs()
    {
        var fromArticleGuid = Guid.NewGuid();

        var command = ArticleEndpointRouteBuilderExtension.CreatePackCommand(
            new ArticleCreateDto(
                new Name("Pack"),
                ArticleType.Pack,
                Pack: new ArticleCreatePackDto(fromArticleGuid, 4.Amount())));

        Assert.Equal(new Name("Pack"), command.Name);
        Assert.Equal(fromArticleGuid, command.FromArticleGuid);
        Assert.True(command.Quantity.Equals(4.Amount(), 0.Amount()));
    }

    [Fact]
    public void CreateKitCommand_Should_MapPacks()
    {
        var firstArticleGuid = Guid.NewGuid();
        var secondArticleGuid = Guid.NewGuid();

        var command = ArticleEndpointRouteBuilderExtension.CreateKitCommand(
            new ArticleCreateDto(
                new Name("Kit"),
                ArticleType.Kit,
                Kit: new ArticleCreateKitDto(
                [
                    new ArticleCreateKitPackDto(firstArticleGuid, 2.Amount()),
                    new ArticleCreateKitPackDto(secondArticleGuid, 3.Amount())
                ])));

        Assert.Collection(
            command.Packs,
            first => Assert.Equal(firstArticleGuid, first.ArticleGuid),
            second => Assert.Equal(secondArticleGuid, second.ArticleGuid));
    }

    [Fact]
    public void CreateContainerCommand_Should_MapMaxMass()
    {
        var command = ArticleEndpointRouteBuilderExtension.CreateContainerCommand(
            new ArticleCreateDto(
                new Name("Container"),
                ArticleType.Container,
                Container: new ArticleCreateContainerDto(Mass.FromKilograms(10))));

        Assert.Equal(new Name("Container"), command.Name);
        Assert.Equal(Mass.FromKilograms(10), command.MaxMass);
    }

    [Fact]
    public void BarcodeRouteConstraint_Should_AcceptBarcodeRouteValue()
    {
        var constraint = new ArticleBarcodeRouteConstraint();
        var values = new RouteValueDictionary
        {
            ["articleBarcode"] = "7891234567895:Ean13"
        };

        var matched = constraint.Match(
            null,
            null,
            "articleBarcode",
            values,
            RouteDirection.IncomingRequest);

        Assert.True(matched);
    }

    [Theory]
    [InlineData("7891234567895")]
    [InlineData("7891234567895:Unknown")]
    [InlineData("123:Ean13")]
    public void BarcodeRouteConstraint_Should_RejectInvalidBarcodeRouteValue(string value)
    {
        var constraint = new ArticleBarcodeRouteConstraint();
        var values = new RouteValueDictionary
        {
            ["articleBarcode"] = value
        };

        var matched = constraint.Match(
            null,
            null,
            "articleBarcode",
            values,
            RouteDirection.IncomingRequest);

        Assert.False(matched);
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
