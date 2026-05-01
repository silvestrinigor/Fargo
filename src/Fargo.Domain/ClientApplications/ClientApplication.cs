namespace Fargo.Domain.ClientApplications;

/// <summary>
/// Represents an external application that authenticates via an API key.
/// </summary>
public class ClientApplication : ModifiedEntity, IActivable
{
    /// <summary>
    /// Display name of the client application.
    /// </summary>
    public required Name Name { get; set; }

    /// <summary>
    /// Optional description of the client application.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    // TODO: Create a KeyHash struct insted of use string.
    /// <summary>
    /// SHA-256 hex hash of the raw API key.
    /// </summary>
    public required string KeyHash { get; set; }

    #region Active

    /// <summary>
    /// Gets a value indicating whether the client application is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Activates the client application.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the client application.
    /// </summary>
    public void Deactivate() => IsActive = false;

    #endregion Active
}
