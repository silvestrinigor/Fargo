using Microsoft.Extensions.Logging;

namespace Fargo.Api.Tests.Integration;

/// <summary>
/// Integration tests for Article CRUD operations. Requires a running Fargo API.
/// Tests are automatically skipped when the server is unreachable.
/// To run: dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
public sealed class ArticleIntegrationTests : IClassFixture<ServerAvailabilityFixture>
{
    private const string Server = "https://localhost:7563";
    private const string ValidUser = "admin";
    private const string ValidPassword = "HJLBaQLIcinDp6KrqRjjgQ@";

    private readonly ServerAvailabilityFixture serverFixture;

    public ArticleIntegrationTests(ServerAvailabilityFixture serverFixture)
    {
        this.serverFixture = serverFixture;
    }

    private void SkipIfUnavailable() =>
        Skip.If(!serverFixture.IsAvailable, $"Fargo API is not running at {Server}");

    private static Engine CreateEngine() =>
        new(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace)));

    [SkippableFact]
    public async Task CreateAsync_Should_ReturnArticle_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);

        var article = await engine.Articles.CreateAsync("Integration Test Article");

        Assert.NotEqual(Guid.Empty, article.Guid);
        Assert.Equal("Integration Test Article", article.Name);

        await engine.Articles.DeleteAsync(article.Guid);
    }

    [SkippableFact]
    public async Task GetAsync_Should_ReturnArticle_When_ArticleExists()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var created = await engine.Articles.CreateAsync("Integration Test Article Get");

        var article = await engine.Articles.GetAsync(created.Guid);

        Assert.Equal(created.Guid, article.Guid);
        Assert.Equal("Integration Test Article Get", article.Name);

        await engine.Articles.DeleteAsync(article.Guid);
    }

    [SkippableFact]
    public async Task GetManyAsync_Should_ReturnArticles_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var created = await engine.Articles.CreateAsync("Integration Test Article List");

        var articles = await engine.Articles.GetManyAsync();

        Assert.NotEmpty(articles);

        await engine.Articles.DeleteAsync(created.Guid);
    }

    [SkippableFact]
    public async Task UpdateAsync_Should_UpdateArticle_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var article = await engine.Articles.CreateAsync("Integration Test Article Update");

        await article.UpdateAsync(a => a.Name = "Integration Test Article Updated");

        Assert.Equal("Integration Test Article Updated", article.Name);

        await engine.Articles.DeleteAsync(article.Guid);
    }

    [SkippableFact]
    public async Task DeleteAsync_Should_DeleteArticle_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var article = await engine.Articles.CreateAsync("Integration Test Article Delete");

        await engine.Articles.DeleteAsync(article.Guid);

        var exception = await Record.ExceptionAsync(() => engine.Articles.GetAsync(article.Guid));
        Assert.IsAssignableFrom<FargoSdkApiException>(exception);
    }
}
