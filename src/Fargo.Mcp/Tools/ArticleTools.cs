using Fargo.Sdk.Articles;
using Fargo.Sdk.Contracts.Articles;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class ArticleTools(IArticleClient articles)
{
    [McpServerTool(Name = "list_articles"), Description("Lists articles accessible to the current user. Optionally filters by partition and public articles.")]
    public async Task<string> ListArticles(
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null,
        [Description("Partition GUIDs whose direct child articles should be included. Omit for no child filter.")] string[]? childOfAnyOfThesePartitions = null,
        [Description("When true, include public articles with no partition.")] bool? notChildOfAnyPartition = null)
    {
        var partitionGuids = childOfAnyOfThesePartitions?.Select(Guid.Parse).ToArray();
        var response = await articles.GetManyAsync(
            page: page,
            limit: limit,
            childOfAnyOfThesePartitions: partitionGuids,
            notChildOfAnyPartition: notChildOfAnyPartition);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var list = response.Result!.Select(a => new { a.Guid, a.Name, a.Description }).ToList();
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

        var article = response.Result!;
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
        var metrics = (massValue ?? lengthXValue ?? lengthYValue ?? lengthZValue) is null
            ? null
            : new ArticleMetricsInfo(
                massValue is null ? null : new MassInfo(massValue.Value, massUnit),
                lengthXValue is null ? null : new LengthInfo(lengthXValue.Value, lengthXUnit),
                lengthYValue is null ? null : new LengthInfo(lengthYValue.Value, lengthYUnit),
                lengthZValue is null ? null : new LengthInfo(lengthZValue.Value, lengthZUnit));

        var response = await articles.CreateAsync(
            new ArticleCreateRequest(
                name,
                Description: description,
                Metrics: metrics,
                ShelfLife: null,
                Partitions: partitions,
                IsActive: isActive));
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        return $"Created article with GUID: {response.Result}";
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

        var current = get.Result!;
        var currentMetrics = current.Metrics;
        var newMetrics = (massValue ?? lengthXValue ?? lengthYValue ?? lengthZValue) is null
            ? currentMetrics
            : new ArticleMetricsInfo(
                massValue is null ? currentMetrics?.Mass : new MassInfo(massValue.Value, massUnit),
                lengthXValue is null ? currentMetrics?.LengthX : new LengthInfo(lengthXValue.Value, lengthXUnit),
                lengthYValue is null ? currentMetrics?.LengthY : new LengthInfo(lengthYValue.Value, lengthYUnit),
                lengthZValue is null ? currentMetrics?.LengthZ : new LengthInfo(lengthZValue.Value, lengthZUnit),
                currentMetrics?.Density);

        var update = await articles.UpdateAsync(
            current.Guid,
            new ArticleUpdateRequest(
                name ?? current.Name,
                description ?? current.Description,
                Metrics: newMetrics,
                ShelfLife: current.ShelfLife,
                Partitions: current.Partitions,
                Ean13: current.Ean13,
                Ean8: current.Ean8,
                UpcA: current.UpcA,
                UpcE: current.UpcE,
                Code128: current.Code128,
                Code39: current.Code39,
                Itf14: current.Itf14,
                Gs1128: current.Gs1128,
                QrCode: current.QrCode,
                DataMatrix: current.DataMatrix,
                IsActive: current.IsActive));

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
