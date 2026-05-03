using Fargo.Api.Articles;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class ArticleTools(IArticleManager articles)
{
    [McpServerTool(Name = "list_articles"), Description("Lists all articles accessible to the current user.")]
    public async Task<string> ListArticles(
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null)
    {
        try
        {
            var result = await articles.GetManyAsync(page: page, limit: limit);
            var list = result.Select(a => new { a.Guid, a.Name, a.Description }).ToList();
            foreach (var a in result)
            {
                await a.DisposeAsync();
            }

            return JsonSerializer.Serialize(list);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_article"), Description("Gets a single article by its GUID.")]
    public async Task<string> GetArticle(
        [Description("The GUID of the article.")] string articleGuid)
    {
        try
        {
            await using var article = await articles.GetAsync(Guid.Parse(articleGuid));
            return JsonSerializer.Serialize(new { article.Guid, article.Name, article.Description, article.Metrics, article.ShelfLife });
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
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
        try
        {
            IReadOnlyCollection<Guid>? partitions = partitionGuids is null ? null : partitionGuids.Select(Guid.Parse).ToArray();
            var metrics = (massValue ?? lengthXValue ?? lengthYValue ?? lengthZValue) is null ? null : new ArticleMetrics
            {
                Mass = massValue is null ? null : new Mass(massValue.Value, Mass.ParseUnit(massUnit)),
                LengthX = lengthXValue is null ? null : new Length(lengthXValue.Value, Length.ParseUnit(lengthXUnit)),
                LengthY = lengthYValue is null ? null : new Length(lengthYValue.Value, Length.ParseUnit(lengthYUnit)),
                LengthZ = lengthZValue is null ? null : new Length(lengthZValue.Value, Length.ParseUnit(lengthZUnit)),
            };
            await using var article = await articles.CreateAsync(name, description, partitions, barcodes: null, metrics: metrics, shelfLife: null, isActive: isActive);
            return $"Created article with GUID: {article.Guid}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
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
        try
        {
            await using var article = await articles.GetAsync(Guid.Parse(articleGuid));
            await article.UpdateAsync(a =>
            {
                if (name is not null)
                {
                    a.Name = name;
                }

                if (description is not null)
                {
                    a.Description = description;
                }

                if (massValue is not null || lengthXValue is not null || lengthYValue is not null || lengthZValue is not null)
                {
                    a.Metrics = new ArticleMetrics
                    {
                        Mass = massValue is null ? a.Metrics?.Mass : new Mass(massValue.Value, Mass.ParseUnit(massUnit)),
                        LengthX = lengthXValue is null ? a.Metrics?.LengthX : new Length(lengthXValue.Value, Length.ParseUnit(lengthXUnit)),
                        LengthY = lengthYValue is null ? a.Metrics?.LengthY : new Length(lengthYValue.Value, Length.ParseUnit(lengthYUnit)),
                        LengthZ = lengthZValue is null ? a.Metrics?.LengthZ : new Length(lengthZValue.Value, Length.ParseUnit(lengthZUnit)),
                    };
                }
            });
            return "Article updated successfully.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "delete_article"), Description("Deletes an article. The article must have no associated items.")]
    public async Task<string> DeleteArticle(
        [Description("The GUID of the article to delete.")] string articleGuid)
    {
        try
        {
            await articles.DeleteAsync(Guid.Parse(articleGuid));
            return "Article deleted successfully.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
