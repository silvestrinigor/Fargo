using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Tests.Integration;

/// <summary>
/// Integration tests for User CRUD operations. Requires a running Fargo API.
/// Tests are automatically skipped when the server is unreachable.
/// To run: dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
public sealed class UserIntegrationTests : IClassFixture<ServerAvailabilityFixture>
{
    private const string Server = "https://localhost:7563";
    private const string ValidUser = "admin";
    private const string ValidPassword = "HJLBaQLIcinDp6KrqRjjgQ@";

    private readonly ServerAvailabilityFixture serverFixture;

    public UserIntegrationTests(ServerAvailabilityFixture serverFixture)
    {
        this.serverFixture = serverFixture;
    }

    private void SkipIfUnavailable() =>
        Skip.If(!serverFixture.IsAvailable, $"Fargo API is not running at {Server}");

    private static Engine CreateEngine() =>
        new(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace)));

    private static string UniqueNameid() => $"inttest-{Guid.NewGuid():N}"[..20];

    [SkippableFact]
    public async Task CreateAsync_Should_ReturnUser_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();

        var user = await engine.Users.CreateAsync(nameid, "TestPass123!");

        Assert.NotEqual(Guid.Empty, user.Guid);
        Assert.Equal(nameid, user.Nameid);

        await engine.Users.DeleteAsync(user.Guid);
    }

    [SkippableFact]
    public async Task GetAsync_Should_ReturnUser_When_UserExists()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();
        var created = await engine.Users.CreateAsync(nameid, "TestPass123!");

        var user = await engine.Users.GetAsync(created.Guid);

        Assert.Equal(created.Guid, user.Guid);
        Assert.Equal(nameid, user.Nameid);

        await engine.Users.DeleteAsync(user.Guid);
    }

    [SkippableFact]
    public async Task GetManyAsync_Should_ReturnUsers_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();
        var created = await engine.Users.CreateAsync(nameid, "TestPass123!");

        var users = await engine.Users.GetManyAsync();

        Assert.NotEmpty(users);

        await engine.Users.DeleteAsync(created.Guid);
    }

    [SkippableFact]
    public async Task UpdateAsync_Should_UpdateUser_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();
        var user = await engine.Users.CreateAsync(nameid, "TestPass123!");
        var updatedNameid = UniqueNameid();

        await user.UpdateAsync(u => u.Nameid = updatedNameid);

        Assert.Equal(updatedNameid, user.Nameid);

        await engine.Users.DeleteAsync(user.Guid);
    }

    [SkippableFact]
    public async Task DeleteAsync_Should_DeleteUser_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();
        var user = await engine.Users.CreateAsync(nameid, "TestPass123!");

        await engine.Users.DeleteAsync(user.Guid);

        var exception = await Record.ExceptionAsync(() => engine.Users.GetAsync(user.Guid));
        Assert.IsAssignableFrom<FargoSdkApiException>(exception);
    }
}
