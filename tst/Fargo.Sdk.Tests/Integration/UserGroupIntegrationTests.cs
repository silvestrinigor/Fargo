using Microsoft.Extensions.Logging;

namespace Fargo.Api.Tests.Integration;

/// <summary>
/// Integration tests for UserGroup CRUD operations. Requires a running Fargo API.
/// Tests are automatically skipped when the server is unreachable.
/// To run: dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
public sealed class UserGroupIntegrationTests : IClassFixture<ServerAvailabilityFixture>
{
    private const string Server = "https://localhost:7563";
    private const string ValidUser = "admin";
    private const string ValidPassword = "HJLBaQLIcinDp6KrqRjjgQ@";

    private readonly ServerAvailabilityFixture serverFixture;

    public UserGroupIntegrationTests(ServerAvailabilityFixture serverFixture)
    {
        this.serverFixture = serverFixture;
    }

    private void SkipIfUnavailable() =>
        Skip.If(!serverFixture.IsAvailable, $"Fargo API is not running at {Server}");

    private static Engine CreateEngine() =>
        new(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace)));

    private static string UniqueNameid() => $"inttest-{Guid.NewGuid():N}"[..20];

    [SkippableFact]
    public async Task CreateAsync_Should_ReturnUserGroup_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();

        var group = await engine.UserGroups.CreateAsync(nameid);

        Assert.NotEqual(Guid.Empty, group.Guid);
        Assert.Equal(nameid, group.Nameid);

        await engine.UserGroups.DeleteAsync(group.Guid);
    }

    [SkippableFact]
    public async Task GetAsync_Should_ReturnUserGroup_When_UserGroupExists()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();
        var created = await engine.UserGroups.CreateAsync(nameid);

        var group = await engine.UserGroups.GetAsync(created.Guid);

        Assert.Equal(created.Guid, group.Guid);
        Assert.Equal(nameid, group.Nameid);

        await engine.UserGroups.DeleteAsync(group.Guid);
    }

    [SkippableFact]
    public async Task GetManyAsync_Should_ReturnUserGroups_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();
        var created = await engine.UserGroups.CreateAsync(nameid);

        var groups = await engine.UserGroups.GetManyAsync();

        Assert.NotEmpty(groups);

        await engine.UserGroups.DeleteAsync(created.Guid);
    }

    [SkippableFact]
    public async Task UpdateAsync_Should_UpdateUserGroup_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();
        var group = await engine.UserGroups.CreateAsync(nameid);
        var updatedNameid = UniqueNameid();

        await group.UpdateAsync(g => g.Nameid = updatedNameid);

        Assert.Equal(updatedNameid, group.Nameid);

        await engine.UserGroups.DeleteAsync(group.Guid);
    }

    [SkippableFact]
    public async Task DeleteAsync_Should_DeleteUserGroup_When_Authenticated()
    {
        SkipIfUnavailable();

        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);
        var nameid = UniqueNameid();
        var group = await engine.UserGroups.CreateAsync(nameid);

        await engine.UserGroups.DeleteAsync(group.Guid);

        var exception = await Record.ExceptionAsync(() => engine.UserGroups.GetAsync(group.Guid));
        Assert.IsAssignableFrom<FargoSdkApiException>(exception);
    }
}
