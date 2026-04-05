namespace Fargo.Sdk.Models;

internal sealed class ArticleInfo
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
