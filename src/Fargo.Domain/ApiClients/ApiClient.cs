namespace Fargo.Domain.ApiClients;

/// <summary>Represents an external application that authenticates via an API key.</summary>
public class ApiClient : ModifiedEntity
{
    /// <summary>Display name of the client application.</summary>
    public required Name Name { get; set; }

    /// <summary>Optional description of the client application.</summary>
    public Description Description { get; set; } = Description.Empty;

    // TODO: Create a KeyHash struct insted of use string.
    /// <summary>SHA-256 hex hash of the raw API key.</summary>
    public required string KeyHash { get; set; }

    /// <summary>Whether this client's key is currently accepted.</summary>
    public bool IsActive { get; set; } = true;
}
