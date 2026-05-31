using Fargo.Core.Shared;
using Fargo.Core.Shared.Barcodes;
using Fargo.HttpApi.Contracts;
using Fargo.Application.Shared.Articles;
using System.Drawing;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;
using AppArticleType = Fargo.Core.Shared.Articles.ArticleType;
using ContractArticleType = Fargo.HttpContracts.ArticleType;
using ContractCreatePackRequest = Fargo.HttpContracts.ArticleCreatePackRequest;
using ContractCreateRequest = Fargo.HttpContracts.ArticleCreateRequest;
using ContractModifiedType = Fargo.HttpContracts.ArticleModifiedType;
using ContractUnitValue = Fargo.HttpContracts.UnitValueDto;

namespace Fargo.HttpApi.Tests.Contracts;

public sealed class HttpContractMappingsTests
{
    [Fact]
    public void ArticleCreateRequest_ToApplication_Should_MapValueObjectsAndQuantities()
    {
        var fromArticleGuid = Guid.NewGuid();
        var request = new ContractCreateRequest(
            "Pack",
            ContractArticleType.Pack,
            Description: "Pack description",
            Metrics: new Fargo.HttpContracts.ArticleMetricsDto(Mass: new ContractUnitValue(10, "kg")),
            Pack: new ContractCreatePackRequest(fromArticleGuid, 4));

        var result = request.ToApplication();

        Assert.Equal(new Name("Pack"), result.Name);
        Assert.Equal(AppArticleType.Pack, result.ArticleType);
        Assert.Equal(new Description("Pack description"), result.Description);
        Assert.Equal(Mass.FromKilograms(10), result.Metrics?.Mass);
        Assert.Equal(fromArticleGuid, result.Pack?.FromArticleGuid);
        Assert.True(result.Pack?.Quantity.Equals(4.Amount(), 0.Amount()));
    }

    [Fact]
    public void ArticleDto_ToContract_Should_MapWirePrimitives()
    {
        var articleGuid = Guid.NewGuid();
        var article = new ArticleDto(
            articleGuid,
            new Name("Article"),
            new Description("Description"),
            ShelfLife: TimeSpan.FromDays(7),
            Color: Color.Red,
            new ArticleMetricsDto(Mass: Mass.FromKilograms(5)),
            new ArticleBarcodesDto(Ean13: new Ean13("7891234567895")),
            Partitions: [],
            IsActive: true,
            EditedByGuid: null);

        var result = article.ToContract();

        Assert.Equal(articleGuid, result.Guid);
        Assert.Equal("Article", result.Name);
        Assert.Equal("Description", result.Description);
        Assert.Equal("red", result.Color?.ToLowerInvariant());
        Assert.Equal("kg", result.Metrics.Mass?.Unit);
        Assert.Equal(5, result.Metrics.Mass?.Value);
        Assert.Equal("7891234567895", result.Barcodes.Ean13);
    }
}
