namespace Fargo.Application.ApiClients;

/// <summary>Configuration options for seeding pre-defined API clients on startup.</summary>
public sealed class ApiClientSeedOptions
{
    /// <summary>The configuration section name used to bind these options.</summary>
    public const string SectionName = "ApiClientSeed";

    /// <summary>Gets or sets the API key for the web API client.</summary>
    public string? WebApiKey { get; set; }

    /// <summary>Gets or sets the API key for the MCP API client.</summary>
    public string? McpApiKey { get; set; }

    /// <summary>Gets or sets the API key for the test API client.</summary>
    public string? TestApiKey { get; set; }

    /// <summary>Gets or sets a value indicating whether the test API client should be seeded.</summary>
    public bool SeedTestClient { get; set; }
}
