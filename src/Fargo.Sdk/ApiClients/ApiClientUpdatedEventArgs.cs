namespace Fargo.Api.ApiClients;

/// <summary>Provides data for the <see cref="ApiClient.Updated"/> event.</summary>
public sealed class ApiClientUpdatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the updated API client.</summary>
    public Guid Guid { get; } = guid;
}
