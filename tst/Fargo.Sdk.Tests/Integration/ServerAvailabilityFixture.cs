namespace Fargo.Api.Tests.Integration;

/// <summary>
/// Probes the Fargo API once per test class run.
/// Tests use <see cref="IsAvailable"/> to skip when the server is unreachable.
/// </summary>
public sealed class ServerAvailabilityFixture : IAsyncLifetime
{
    private const string ProbeUrl = "https://localhost:7563";

    public bool IsAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            using var http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            await http.GetAsync(ProbeUrl);
            IsAvailable = true;
        }
        catch
        {
            IsAvailable = false;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
