using Fargo.Application.Storage;
using Fargo.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Fargo.Infrastructure.Storage;

/// <summary>
/// Local file system implementation of <see cref="IArticleImageStorage"/>.
/// Images are stored under <c>{BasePath}/articles/{articleGuid}.{ext}</c>.
/// </summary>
/// <remarks>
/// The storage key (persisted in the database) is the path relative to <c>BasePath</c>,
/// for example <c>articles/3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg</c>.
/// This key format is kept provider-agnostic so the same value can be used
/// as an S3 object key or a MinIO key when migrating storage backends.
/// </remarks>
public sealed class LocalArticleImageStorage(IOptions<ArticleImageOptions> options) : IArticleImageStorage
{
    private static readonly Dictionary<string, string> _contentTypeToExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/gif"] = ".gif",
        ["image/webp"] = ".webp",
        ["image/bmp"] = ".bmp",
        ["image/tiff"] = ".tiff",
        ["image/svg+xml"] = ".svg",
    };

    private static readonly Dictionary<string, string> _extensionToContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".bmp"] = "image/bmp",
        [".tiff"] = "image/tiff",
        [".svg"] = "image/svg+xml",
    };

    private string BasePath => options.Value.BasePath;

    /// <inheritdoc/>
    public async Task<string> SaveAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var ext = _contentTypeToExtension.TryGetValue(contentType, out var mapped) ? mapped : ".bin";
        var key = $"articles/{articleGuid}{ext}";
        var fullPath = GetFullPath(key);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await stream.CopyToAsync(fileStream, cancellationToken);

        return key;
    }

    /// <inheritdoc/>
    public Task<(Stream Stream, string ContentType)?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(key);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<(Stream, string)?>(null);
        }

        var ext = Path.GetExtension(fullPath);
        var contentType = _extensionToContentType.TryGetValue(ext, out var ct) ? ct : "application/octet-stream";

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult<(Stream, string)?>((stream, contentType));
    }

    /// <inheritdoc/>
    public Task DeleteAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(key);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string key) => Path.Combine(BasePath, key);
}
