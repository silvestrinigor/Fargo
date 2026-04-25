using Fargo.Sdk.Articles;
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
            return JsonSerializer.Serialize(new { article.Guid, article.Name, article.Description, article.Mass, article.LengthX, article.LengthY, article.LengthZ });
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
        [Description("GUID of the partition to associate with the article. Omit to create a public article with no partition.")] string? partitionGuid = null,
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
            Guid? firstPartition = partitionGuid is not null ? Guid.Parse(partitionGuid) : null;
            var mass = massValue is null ? null : new MassDto(massValue.Value, massUnit);
            var lengthX = lengthXValue is null ? null : new LengthDto(lengthXValue.Value, lengthXUnit);
            var lengthY = lengthYValue is null ? null : new LengthDto(lengthYValue.Value, lengthYUnit);
            var lengthZ = lengthZValue is null ? null : new LengthDto(lengthZValue.Value, lengthZUnit);
            await using var article = await articles.CreateAsync(name, description, firstPartition, mass, lengthX, lengthY, lengthZ);
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
                    a.Name = name;

                if (description is not null)
                    a.Description = description;

                if (massValue is not null)
                    a.Mass = new MassDto(massValue.Value, massUnit);

                if (lengthXValue is not null)
                    a.LengthX = new LengthDto(lengthXValue.Value, lengthXUnit);

                if (lengthYValue is not null)
                    a.LengthY = new LengthDto(lengthYValue.Value, lengthYUnit);

                if (lengthZValue is not null)
                    a.LengthZ = new LengthDto(lengthZValue.Value, lengthZUnit);
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
