using Fargo.Sdk;
using Fargo.Sdk.Authentication;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Tests.Integration;

/// <summary>
/// Integration tests that require a running Fargo API.
/// Tests are automatically skipped when the server is unreachable.
/// To run: dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
public sealed class AuthenticationIntegrationTests : IClassFixture<ServerAvailabilityFixture>
{
    private const string Server = "https://localhost:7563";
    private const string ValidUser = "admin";
    private const string ValidPassword = "HJLBaQLIcinDp6KrqRjjgQ@";

    private readonly ServerAvailabilityFixture serverFixture;

    public AuthenticationIntegrationTests(ServerAvailabilityFixture serverFixture)
    {
        this.serverFixture = serverFixture;
    }

    private void SkipIfUnavailable() =>
        Skip.If(!serverFixture.IsAvailable, $"Fargo API is not running at {Server}");

    private static Engine CreateEngine() =>
        new(LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace)));

    [SkippableFact]
    public async Task LogInAsync_Should_Authenticate_When_CredentialsAreValid()
    {
        SkipIfUnavailable();

        // Arrange
        using var engine = CreateEngine();

        // Act
        await engine.LogInAsync(Server, ValidUser, ValidPassword);

        // Assert
        Assert.True(engine.Authentication.IsAuthenticated);
        Assert.Equal(ValidUser, engine.Authentication.Session.Nameid);
    }

    [SkippableFact]
    public async Task LogOutAsync_Should_ClearSession_After_Login()
    {
        SkipIfUnavailable();

        // Arrange
        using var engine = CreateEngine();
        await engine.LogInAsync(Server, ValidUser, ValidPassword);

        // Act
        await engine.LogOutAsync();

        // Assert
        Assert.False(engine.Authentication.IsAuthenticated);
    }

    [SkippableFact]
    public async Task LogInAsync_Should_ThrowAndNotAuthenticate_When_CredentialsAreWrong()
    {
        SkipIfUnavailable();

        // Arrange
        using var engine = CreateEngine();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            engine.LogInAsync(Server, ValidUser, ValidPassword + "_wrong"));

        // Assert — SDK always throws a FargoSdkException on failure
        Assert.IsAssignableFrom<FargoSdkException>(exception);
        Assert.False(engine.Authentication.IsAuthenticated);
    }

    [SkippableFact]
    public async Task LogInAsync_Should_ThrowConnectionException_When_ServerIsUnreachable()
    {
        SkipIfUnavailable();

        // Arrange
        using var engine = CreateEngine();

        // Act / Assert
        await Assert.ThrowsAsync<FargoSdkConnectionException>(() =>
            engine.LogInAsync("https://localhost:9999", ValidUser, ValidPassword));
    }
}
