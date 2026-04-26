using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Tests.Integration;

/// <summary>
/// Integration tests for Item CRUD operations. Requires a running Fargo API.
/// Tests are automatically skipped when the server is unreachable.
/// To run: dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
public sealed class ItemIntegrationTests : IClassFixture<ServerAvailabilityFixture>
{
    private const string Server = "https://localhost:7563";
    private const string ValidUser = "admin";
    private const string ValidPassword = "HJLBaQLIcinDp6KrqRjjgQ@";

    private readonly ServerAvailabilityFixture serverFixture;

    public ItemIntegrationTests(ServerAvailabilityFixture serverFixture)
    {
        this.serverFixture = serverFixture;
    }

    private void SkipIfUnavailable() =>
        Skip.If(!serverFixture.IsAvailable, $"Fargo API is not running at {Server}");

    private static Engine CreateEngine() =>
        new(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace)));

    [SkippableFact]
    public async Task CreateAsync_Should_ReturnItem_When_ArticleExists()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var article = await engine.Articles.CreateAsync("Integration Test Article For Item");

        var item = await engine.Items.CreateAsync(article.Guid);

        Assert.NotEqual(Guid.Empty, item.Guid);
        Assert.Equal(article.Guid, item.ArticleGuid);

        await engine.Items.DeleteAsync(item.Guid);
        await engine.Articles.DeleteAsync(article.Guid);
    }

    [SkippableFact]
    public async Task GetAsync_Should_ReturnItem_When_ItemExists()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var article = await engine.Articles.CreateAsync("Integration Test Article For Item Get");
        var created = await engine.Items.CreateAsync(article.Guid);

        var item = await engine.Items.GetAsync(created.Guid);

        Assert.Equal(created.Guid, item.Guid);
        Assert.Equal(article.Guid, item.ArticleGuid);

        await engine.Items.DeleteAsync(item.Guid);
        await engine.Articles.DeleteAsync(article.Guid);
    }

    [SkippableFact]
    public async Task GetManyAsync_Should_ReturnItems_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var article = await engine.Articles.CreateAsync("Integration Test Article For Item List");
        var created = await engine.Items.CreateAsync(article.Guid);

        var items = await engine.Items.GetManyAsync(article.Guid);

        Assert.NotEmpty(items);

        await engine.Items.DeleteAsync(created.Guid);
        await engine.Articles.DeleteAsync(article.Guid);
    }

    [SkippableFact]
    public async Task DeleteAsync_Should_DeleteItem_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var article = await engine.Articles.CreateAsync("Integration Test Article For Item Delete");
        var item = await engine.Items.CreateAsync(article.Guid);

        await engine.Items.DeleteAsync(item.Guid);

        var exception = await Record.ExceptionAsync(() => engine.Items.GetAsync(item.Guid));
        Assert.IsAssignableFrom<FargoSdkApiException>(exception);

        await engine.Articles.DeleteAsync(article.Guid);
    }
}
