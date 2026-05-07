using Fargo.Sdk.Articles;
using Fargo.Sdk.Contracts.Articles;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class ArticleTools(IArticleHttpClient articles)
{
    [McpServerTool(Name = "list_articles"), Description("Lists articles accessible to the current user. Optionally filters by partition and public articles.")]
    public async Task<string> ListArticles(
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null,
        [Description("Partition GUIDs to include. Omit for no partition filter.")] string[]? insideAnyOfThisPartitions = null,
        [Description("When true, include public articles with no partition.")] bool? notInsideAnyPartition = null)
    {
        var partitionGuids = insideAnyOfThisPartitions?.Select(Guid.Parse).ToArray();
        var response = await articles.GetManyAsync(
            page: page,
            limit: limit,
            insideAnyOfThisPartitions: partitionGuids,
            notInsideAnyPartition: notInsideAnyPartition);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var list = response.Data!.Select(a => new { a.Guid, a.Name, a.Description }).ToList();
        return JsonSerializer.Serialize(list);
    }

    [McpServerTool(Name = "get_article"), Description("Gets a single article by its GUID.")]
    public async Task<string> GetArticle(
        [Description("The GUID of the article.")] string articleGuid)
    {
        var response = await articles.GetAsync(Guid.Parse(articleGuid));
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var article = response.Data!;
        return JsonSerializer.Serialize(new { article.Guid, article.Name, article.Description, article.Metrics, article.ShelfLife });
    }

    [McpServerTool(Name = "create_article"), Description("Creates a new article.")]
    public async Task<string> CreateArticle(
        [Description("The display name of the article.")] string name,
        [Description("An optional description.")] string? description = null,
        [Description("GUIDs of partitions to associate with the article. Omit for a public article with no partition.")] string[]? partitionGuids = null,
        [Description("Whether the article should be active. Defaults to true.")] bool? isActive = null,
        [Description("Mass value of the article. Omit if unknown.")] double? massValue = null,
        [Description("Mass unit (g, kg, mg, lb, oz). Defaults to g.")] string massUnit = "g",
        [Description("LengthX dimension value of the article. Omit if unknown.")] double? lengthXValue = null,
        [Description("LengthX dimension unit (mm, cm, m, km, in, ft). Defaults to m.")] string lengthXUnit = "m",
        [Description("LengthY dimension value of the article. Omit if unknown.")] double? lengthYValue = null,
        [Description("LengthY dimension unit (mm, cm, m, km, in, ft). Defaults to m.")] string lengthYUnit = "m",
        [Description("LengthZ dimension value of the article. Omit if unknown.")] double? lengthZValue = null,
        [Description("LengthZ dimension unit (mm, cm, m, km, in, ft). Defaults to m.")] string lengthZUnit = "m")
    {
        IReadOnlyCollection<Guid>? partitions = partitionGuids?.Select(Guid.Parse).ToArray();
        var metrics = (massValue ?? lengthXValue ?? lengthYValue ?? lengthZValue) is null ? null : new ArticleMetrics
        {
            Mass = massValue is null ? null : new Mass(massValue.Value, Mass.ParseUnit(massUnit)),
            LengthX = lengthXValue is null ? null : new Length(lengthXValue.Value, Length.ParseUnit(lengthXUnit)),
            LengthY = lengthYValue is null ? null : new Length(lengthYValue.Value, Length.ParseUnit(lengthYUnit)),
            LengthZ = lengthZValue is null ? null : new Length(lengthZValue.Value, Length.ParseUnit(lengthZUnit)),
        };

        var response = await articles.CreateAsync(name, description, partitions, barcodes: null, metrics: metrics, shelfLife: null, isActive: isActive);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        return $"Created article with GUID: {response.Data}";
    }

    [McpServerTool(Name = "update_article"), Description("Updates an article's name, description, mass, and/or dimensions.")]
    public async Task<string> UpdateArticle(
        [Description("The GUID of the article to update.")] string articleGuid,
        [Description("The new name. Omit to leave unchanged.")] string? name = null,
        [Description("The new description. Omit to leave unchanged.")] string? description = null,
        [Description("New mass value. Omit to leave unchanged.")] double? massValue = null,
        [Description("Mass unit (g, kg, mg, lb, oz). Defaults to g.")] string massUnit = "g",
        [Description("New LengthX dimension value. Omit to leave unchanged.")] double? lengthXValue = null,
        [Description("LengthX dimension unit (mm, cm, m, km, in, ft). Defaults to m.")] string lengthXUnit = "m",
        [Description("New LengthY dimension value. Omit to leave unchanged.")] double? lengthYValue = null,
        [Description("LengthY dimension unit (mm, cm, m, km, in, ft). Defaults to m.")] string lengthYUnit = "m",
        [Description("New LengthZ dimension value. Omit to leave unchanged.")] double? lengthZValue = null,
        [Description("LengthZ dimension unit (mm, cm, m, km, in, ft). Defaults to m.")] string lengthZUnit = "m")
    {
        var get = await articles.GetAsync(Guid.Parse(articleGuid));
        if (!get.IsSuccess)
        {
            return $"Error: {get.Error!.Detail}";
        }

        var current = get.Data!;
        var currentMetrics = current.Metrics.ToSdkArticleMetrics();
        var newMetrics = (massValue ?? lengthXValue ?? lengthYValue ?? lengthZValue) is null ? currentMetrics : new ArticleMetrics
        {
            Mass = massValue is null ? currentMetrics?.Mass : new Mass(massValue.Value, Mass.ParseUnit(massUnit)),
            LengthX = lengthXValue is null ? currentMetrics?.LengthX : new Length(lengthXValue.Value, Length.ParseUnit(lengthXUnit)),
            LengthY = lengthYValue is null ? currentMetrics?.LengthY : new Length(lengthYValue.Value, Length.ParseUnit(lengthYUnit)),
            LengthZ = lengthZValue is null ? currentMetrics?.LengthZ : new Length(lengthZValue.Value, Length.ParseUnit(lengthZUnit)),
        };

        var update = await articles.UpdateAsync(
            current.Guid,
            name ?? current.Name,
            description ?? current.Description,
            current.Partitions,
            current.Barcodes.ToSdkArticleBarcodes(),
            newMetrics,
            current.ShelfLife,
            current.IsActive);

        if (!update.IsSuccess)
        {
            return $"Error: {update.Error!.Detail}";
        }

        return "Article updated successfully.";
    }

    [McpServerTool(Name = "delete_article"), Description("Deletes an article. The article must have no associated items.")]
    public async Task<string> DeleteArticle(
        [Description("The GUID of the article to delete.")] string articleGuid)
    {
        var response = await articles.DeleteAsync(Guid.Parse(articleGuid));
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        return "Article deleted successfully.";
    }
}

internal static class ArticleToolContractMappings
{
    public static ArticleMetrics? ToSdkArticleMetrics(this ArticleMetricsInfo? contract)
        => contract is null
            ? null
            : new ArticleMetrics
            {
                Mass = contract.Mass is null ? null : new Mass(contract.Mass.Value, Mass.ParseUnit(contract.Mass.Unit)),
                LengthX = contract.LengthX is null ? null : new Length(contract.LengthX.Value, Length.ParseUnit(contract.LengthX.Unit)),
                LengthY = contract.LengthY is null ? null : new Length(contract.LengthY.Value, Length.ParseUnit(contract.LengthY.Unit)),
                LengthZ = contract.LengthZ is null ? null : new Length(contract.LengthZ.Value, Length.ParseUnit(contract.LengthZ.Unit)),
            };

    public static ArticleBarcodes ToSdkArticleBarcodes(this ArticleBarcodesInfo? contract)
        => contract is null
            ? new ArticleBarcodes()
            : new ArticleBarcodes
            {
                Ean13 = contract.Ean13 is null ? null : new Ean13(Guid.Empty, Guid.Empty, contract.Ean13),
                Ean8 = contract.Ean8 is null ? null : new Ean8(Guid.Empty, Guid.Empty, contract.Ean8),
                UpcA = contract.UpcA is null ? null : new UpcA(Guid.Empty, Guid.Empty, contract.UpcA),
                UpcE = contract.UpcE is null ? null : new UpcE(Guid.Empty, Guid.Empty, contract.UpcE),
                Code128 = contract.Code128 is null ? null : new Code128(Guid.Empty, Guid.Empty, contract.Code128),
                Code39 = contract.Code39 is null ? null : new Code39(Guid.Empty, Guid.Empty, contract.Code39),
                Itf14 = contract.Itf14 is null ? null : new Itf14(Guid.Empty, Guid.Empty, contract.Itf14),
                Gs1128 = contract.Gs1128 is null ? null : new Gs1128(Guid.Empty, Guid.Empty, contract.Gs1128),
                QrCode = contract.QrCode is null ? null : new QrCode(Guid.Empty, Guid.Empty, contract.QrCode),
                DataMatrix = contract.DataMatrix is null ? null : new DataMatrix(Guid.Empty, Guid.Empty, contract.DataMatrix),
            };
}
