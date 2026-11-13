using Fargo.ClientHttp.Authentication;
using Microsoft.Extensions.Time.Testing;
using System.Net;
using System.Text.Json;

namespace Fargo.ClientHttp.Tests;

public class FargoTokenRefresherTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeRefreshHandler handler = new();
    private readonly TokenStore store = new();
    private readonly FakeTimeProvider clock = new(Now);

    private FargoTokenRefresher CreateRefresher(string baseAddress = "http://api.test/")
        => new(new HttpClient(handler) { BaseAddress = new Uri(baseAddress) }, store, clock);

    private Task StoreTokensExpiringInAsync(TimeSpan expiresIn, string refreshToken = "old-refresh")
        => store.SetAsync(new AuthTokens("old-access", refreshToken, Now + expiresIn));

    [Fact]
    public async Task EnsureValidAsync_WhenTheTokenIsStillValid_ShouldNotCallTheApi()
    {
        await StoreTokensExpiringInAsync(TimeSpan.FromMinutes(10));

        var refreshed = await CreateRefresher().EnsureValidAsync();

        Assert.False(refreshed);
        Assert.Equal(0, handler.CallCount);
        Assert.Equal("old-access", (await store.GetAsync())!.AccessToken);
    }

    [Theory]
    [InlineData(-60)]
    [InlineData(10)]
    [InlineData(30)]
    public async Task EnsureValidAsync_WhenExpiredOrWithinTheBuffer_ShouldRefreshAndStoreTheNewTokens(int expiresInSeconds)
    {
        await StoreTokensExpiringInAsync(TimeSpan.FromSeconds(expiresInSeconds));

        var refreshed = await CreateRefresher().EnsureValidAsync();

        Assert.True(refreshed);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(
            new AuthTokens("new-access", "new-refresh", new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            await store.GetAsync());
    }

    [Fact]
    public async Task EnsureValidAsync_ShouldPostTheStoredRefreshTokenToTheRefreshEndpoint()
    {
        await StoreTokensExpiringInAsync(TimeSpan.Zero, "old-refresh");

        await CreateRefresher().EnsureValidAsync();

        var (uri, body) = Assert.Single(handler.Requests);
        Assert.Equal(new Uri("http://api.test/v1/identity/refresh"), uri);
        Assert.Equal("""{"refreshToken":"old-refresh"}""", body);
    }

    [Theory]
    [InlineData("http://api.test", "http://api.test/v1/identity/refresh")]
    [InlineData("http://api.test/fargo", "http://api.test/fargo/v1/identity/refresh")]
    [InlineData("http://api.test/fargo/", "http://api.test/fargo/v1/identity/refresh")]
    public async Task EnsureValidAsync_ShouldKeepThePathOfTheBaseAddress(string baseAddress, string expected)
    {
        await StoreTokensExpiringInAsync(TimeSpan.Zero);

        await CreateRefresher(baseAddress).EnsureValidAsync();

        Assert.Equal(new Uri(expected), Assert.Single(handler.Requests).Uri);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task EnsureValidAsync_WhenTheApiRejectsTheRefresh_ShouldLeaveTheStoreUnchanged(HttpStatusCode status)
    {
        await StoreTokensExpiringInAsync(TimeSpan.Zero);
        var before = await store.GetAsync();
        handler.StatusCode = status;

        var refreshed = await CreateRefresher().EnsureValidAsync();

        Assert.False(refreshed);
        Assert.Equal(before, await store.GetAsync());
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{"accessToken":"a"}""")]
    [InlineData("""{"accessToken":"","refreshToken":"r","expiresAt":"2030-01-01T00:00:00+00:00"}""")]
    public async Task EnsureValidAsync_WhenTheResponseIsIncomplete_ShouldLeaveTheStoreUnchanged(string responseBody)
    {
        await StoreTokensExpiringInAsync(TimeSpan.Zero);
        var before = await store.GetAsync();
        handler.ResponseBody = responseBody;

        Assert.False(await CreateRefresher().EnsureValidAsync());
        Assert.Equal(before, await store.GetAsync());
    }

    [Fact]
    public async Task EnsureValidAsync_WhenThereAreNoTokens_ShouldDoNothing()
    {
        Assert.False(await CreateRefresher().EnsureValidAsync());
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task EnsureValidAsync_WhenCalledConcurrently_ShouldRefreshOnce()
    {
        // The API revokes a refresh token on use: a second refresh with the
        // same token would fail and log the session out.
        await StoreTokensExpiringInAsync(TimeSpan.Zero);
        handler.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var refresher = CreateRefresher();

        var calls = Enumerable.Range(0, 5).Select(_ => refresher.EnsureValidAsync()).ToList();

        await WaitUntilAsync(() => handler.CallCount >= 1);
        handler.Gate.SetResult();
        var results = await Task.WhenAll(calls);

        Assert.Equal(1, handler.CallCount);
        Assert.Equal(1, results.Count(r => r));
        Assert.Equal("new-access", (await store.GetAsync())!.AccessToken);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRefreshEvenWhenTheTokenIsValid()
    {
        await StoreTokensExpiringInAsync(TimeSpan.FromHours(1));

        Assert.True(await CreateRefresher().RefreshAsync());
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task RefreshAsync_WhenCalledConcurrently_ShouldRefreshOnceAndReportSuccessToAll()
    {
        await StoreTokensExpiringInAsync(TimeSpan.FromHours(1));
        handler.Gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var refresher = CreateRefresher();

        var calls = Enumerable.Range(0, 3).Select(_ => refresher.RefreshAsync()).ToList();

        await WaitUntilAsync(() => handler.CallCount >= 1);
        handler.Gate.SetResult();
        var results = await Task.WhenAll(calls);

        Assert.Equal(1, handler.CallCount);
        Assert.All(results, Assert.True);
    }

    [Fact]
    public async Task RefreshingAccessTokenSource_ShouldRefreshBeforeReturningTheToken()
    {
        await StoreTokensExpiringInAsync(TimeSpan.Zero);
        var source = new RefreshingAccessTokenSource(CreateRefresher(), store);

        Assert.Equal("new-access", await source.GetAccessTokenAsync());
    }

    [Fact]
    public async Task StoredAccessTokenSource_ShouldNeverRefresh()
    {
        await StoreTokensExpiringInAsync(TimeSpan.FromSeconds(-60));
        var source = new StoredAccessTokenSource(store);

        Assert.Equal("old-access", await source.GetAccessTokenAsync());
        Assert.Equal(0, handler.CallCount);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);

        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the condition.");

            await Task.Delay(10);
        }
    }
}

/// <summary>
/// Guards the hand-written refresh call against the API's OpenAPI document,
/// which is the safety the generated Kiota client used to provide.
/// </summary>
public class RefreshContractTests
{
    private static JsonElement LoadDocument()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fargo.ServiceHttp.json");

        return JsonDocument.Parse(File.ReadAllText(path)).RootElement;
    }

    private static JsonElement ResolveSchema(JsonElement document, JsonElement schema)
    {
        var reference = schema.GetProperty("$ref").GetString()!;

        return document.GetProperty("components").GetProperty("schemas").GetProperty(reference.Split('/')[^1]);
    }

    [Fact]
    public void TheRefreshEndpointShouldExistAtTheExpectedPath()
    {
        var paths = LoadDocument().GetProperty("paths");

        Assert.True(paths.TryGetProperty("/" + FargoTokenRefresher.RefreshPath, out var path));
        Assert.True(path.TryGetProperty("post", out _));
    }

    [Fact]
    public void TheRequestShouldBeAnObjectWithOnlyARequiredRefreshToken()
    {
        var document = LoadDocument();
        var post = document.GetProperty("paths").GetProperty("/" + FargoTokenRefresher.RefreshPath).GetProperty("post");
        var schema = ResolveSchema(
            document,
            post.GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("schema"));

        Assert.Equal(["refreshToken"], schema.GetProperty("required").EnumerateArray().Select(e => e.GetString()));
        Assert.Equal(["refreshToken"], schema.GetProperty("properties").EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public void TheResponseShouldHaveTheTokensAndTheExpiry()
    {
        var document = LoadDocument();
        var post = document.GetProperty("paths").GetProperty("/" + FargoTokenRefresher.RefreshPath).GetProperty("post");
        var schema = ResolveSchema(
            document,
            post.GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema"));

        var required = schema.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToHashSet();
        Assert.Superset(new HashSet<string?> { "accessToken", "refreshToken", "expiresAt" }, required);
        Assert.Equal("date-time", schema.GetProperty("properties").GetProperty("expiresAt").GetProperty("format").GetString());
    }
}
