namespace Fargo.Application.ApiClients;

public sealed class ApiClientSeedOptions
{
    public const string SectionName = "ApiClientSeed";

    public string? WebApiKey { get; set; }
    public string? McpApiKey { get; set; }
    public string? TestApiKey { get; set; }
    public bool SeedTestClient { get; set; }
}
