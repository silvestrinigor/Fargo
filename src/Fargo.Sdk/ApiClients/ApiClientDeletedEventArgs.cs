namespace Fargo.Sdk.ApiClients;

/// <summary>Provides data for the <see cref="ApiClient.Deleted"/> event.</summary>
public sealed class ApiClientDeletedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the deleted API client.</summary>
    public Guid Guid { get; } = guid;
}
