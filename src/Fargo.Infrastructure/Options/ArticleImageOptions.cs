namespace Fargo.Infrastructure.Options;

/// <summary>
/// Configuration options for article image storage.
/// </summary>
public sealed class ArticleImageOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "ArticleImage";

    /// <summary>
    /// The base directory where article images are stored when using the local file system provider.
    /// Defaults to <c>./images</c> relative to the application working directory.
    /// </summary>
    public string BasePath { get; set; } = "./images";
}
