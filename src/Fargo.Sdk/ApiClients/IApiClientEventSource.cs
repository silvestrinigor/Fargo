namespace Fargo.Sdk.ApiClients;

/// <summary>Broadcasts the <see cref="Created"/> event when a new API client is created by any client.</summary>
public interface IApiClientEventSource
{
    event EventHandler<ApiClientCreatedEventArgs> Created;
}
