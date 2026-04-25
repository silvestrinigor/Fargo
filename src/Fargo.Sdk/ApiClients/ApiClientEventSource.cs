using Fargo.Sdk.Events;

namespace Fargo.Sdk.ApiClients;

/// <summary>Default implementation of <see cref="IApiClientEventSource"/>.</summary>
public sealed class ApiClientEventSource : IApiClientEventSource
{
    public ApiClientEventSource(IFargoEventHub hub)
    {
        hub.On<Guid>("OnApiClientCreated", guid =>
            Created?.Invoke(this, new ApiClientCreatedEventArgs(guid)));
    }

    public event EventHandler<ApiClientCreatedEventArgs>? Created;
}
