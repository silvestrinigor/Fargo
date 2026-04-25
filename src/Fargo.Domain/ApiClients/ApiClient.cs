namespace Fargo.Domain.ApiClients;

public class ApiClient : ModifiedEntity
{
    public required Name Name { get; set; }
    public Description Description { get; set; } = Description.Empty;
    /// <summary>SHA-256 hex hash of the raw API key.</summary>
    public required string KeyHash { get; set; }
    public bool IsActive { get; set; } = true;
}
