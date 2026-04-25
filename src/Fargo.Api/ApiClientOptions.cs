namespace Fargo.Api;

public sealed class ApiClientOptions
{
    public const string SectionName = "ApiClient";
    public bool EnforceApiClient { get; set; } = false;
}
