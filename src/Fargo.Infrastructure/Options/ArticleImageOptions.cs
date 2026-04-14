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
    /// Defaults to <c>{LocalApplicationData}/Fargo/images</c>
    /// (<c>~/.local/share/Fargo/images</c> on Linux/macOS, <c>%LOCALAPPDATA%\Fargo\images</c> on Windows).
    /// Override via <c>ArticleImage:BasePath</c> in configuration.
    /// </summary>
    public string BasePath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Fargo",
        "images");
}
