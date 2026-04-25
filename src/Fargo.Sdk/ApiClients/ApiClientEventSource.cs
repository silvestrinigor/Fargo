using Fargo.Sdk.Events;

namespace Fargo.Sdk.ApiClients;

/// <summary>Default implementation of <see cref="IApiClientEventSource"/>.</summary>
public sealed class ApiClientEventSource : IApiClientEventSource
{
    /// <summary>Initializes a new instance and subscribes to the hub's <c>OnApiClientCreated</c> event.</summary>
    /// <param name="hub">The event hub to subscribe to.</param>
    public ApiClientEventSource(IFargoEventHub hub)
    {
        hub.On<Guid>("OnApiClientCreated", guid =>
            Created?.Invoke(this, new ApiClientCreatedEventArgs(guid)));
    }

    /// <inheritdoc />
    public event EventHandler<ApiClientCreatedEventArgs>? Created;
}
