namespace Fargo.Sdk.ApiClients;

/// <summary>Provides data for the <see cref="IApiClientEventSource.Created"/> event.</summary>
public sealed class ApiClientCreatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the created API client.</summary>
    public Guid Guid { get; } = guid;
}
