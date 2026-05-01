namespace Fargo.Api.ApiClients;

/// <summary>Broadcasts the <see cref="Created"/> event when a new API client is created by any client.</summary>
public interface IApiClientEventSource
{
    /// <summary>Raised when any authenticated client creates an API client.</summary>
    event EventHandler<ApiClientCreatedEventArgs> Created;
}
