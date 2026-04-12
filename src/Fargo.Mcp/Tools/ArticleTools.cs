using Fargo.Sdk;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class ArticleTools(IEngine engine)
{
    [McpServerTool(Name = "list_articles"), Description("Lists all articles accessible to the current user.")]
    public async Task<string> ListArticles(
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null)
    {
        try
        {
            var articles = await engine.Articles.GetManyAsync(page: page, limit: limit);
            var results = articles.Select(a => new { a.Guid, a.Name, a.Description }).ToList();
            foreach (var a in articles)
            {
                await a.DisposeAsync();
            }

            return JsonSerializer.Serialize(results);
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
            await using var article = await engine.Articles.GetAsync(Guid.Parse(articleGuid));
            return JsonSerializer.Serialize(new { article.Guid, article.Name, article.Description, article.Mass });
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
        [Description("GUID of the partition to associate with the article. Omit to use the global partition.")] string? partitionGuid = null,
        [Description("Mass value of the article. Omit if unknown.")] double? massValue = null,
        [Description("Mass unit (g, kg, mg, lb, oz). Defaults to g.")] string massUnit = "g")
    {
        try
        {
            Guid? firstPartition = partitionGuid is not null ? Guid.Parse(partitionGuid) : null;
            var mass = massValue is null ? null : new Fargo.Sdk.Articles.MassDto(massValue.Value, massUnit);
            await using var article = await engine.Articles.CreateAsync(name, description, firstPartition, mass);
            return $"Created article with GUID: {article.Guid}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "update_article"), Description("Updates an article's name, description, and/or mass.")]
    public async Task<string> UpdateArticle(
        [Description("The GUID of the article to update.")] string articleGuid,
        [Description("The new name. Omit to leave unchanged.")] string? name = null,
        [Description("The new description. Omit to leave unchanged.")] string? description = null,
        [Description("New mass value. Omit to leave unchanged.")] double? massValue = null,
        [Description("Mass unit (g, kg, mg, lb, oz). Defaults to g.")] string massUnit = "g")
    {
        try
        {
            await using var article = await engine.Articles.GetAsync(Guid.Parse(articleGuid));
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

                if (massValue is not null)
                {
                    a.Mass = new Fargo.Sdk.Articles.MassDto(massValue.Value, massUnit);
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
            await engine.Articles.DeleteAsync(Guid.Parse(articleGuid));
            return "Article deleted successfully.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
