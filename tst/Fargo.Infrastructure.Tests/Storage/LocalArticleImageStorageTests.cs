using Fargo.Infrastructure.Options;
using Fargo.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace Fargo.Infrastructure.Tests.Storage;

public sealed class LocalArticleImageStorageTests : IDisposable
{
    private readonly string basePath = Path.Combine(Path.GetTempPath(), "fargo-image-tests", Guid.NewGuid().ToString());

    [Fact]
    public async Task SaveAsync_ShouldGenerateUniqueKeys_ForSameArticle()
    {
        var sut = new LocalArticleImageStorage(Microsoft.Extensions.Options.Options.Create(new ArticleImageOptions
        {
            BasePath = basePath
        }));
        var articleGuid = Guid.NewGuid();

        var firstKey = await sut.SaveAsync(articleGuid, new MemoryStream([1, 2, 3]), "image/png");
        var secondKey = await sut.SaveAsync(articleGuid, new MemoryStream([4, 5, 6]), "image/png");

        Assert.NotEqual(firstKey, secondKey);
        Assert.StartsWith($"articles/{articleGuid}/", firstKey, StringComparison.Ordinal);
        Assert.StartsWith($"articles/{articleGuid}/", secondKey, StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(basePath, firstKey)));
        Assert.True(File.Exists(Path.Combine(basePath, secondKey)));
    }

    public void Dispose()
    {
        if (Directory.Exists(basePath))
        {
            Directory.Delete(basePath, recursive: true);
        }
    }
}
