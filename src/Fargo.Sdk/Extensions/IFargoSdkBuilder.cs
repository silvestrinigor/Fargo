using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Api.Extensions;

/// <summary>Builder returned by <see cref="FargoSdkServiceCollectionExtensions.AddFargoSdk"/> for optional configuration.</summary>
public interface IFargoSdkBuilder
{
    /// <summary>Gets the underlying service collection.</summary>
    IServiceCollection Services { get; }

    /// <summary>Registers <see cref="Authentication.IAuthenticationService"/> events to automatically connect/disconnect the hub on login and logout.</summary>
    IFargoSdkBuilder WithHubLifetime();

    /// <summary>Registers a custom session store so authentication sessions survive page reloads.</summary>
    IFargoSdkBuilder WithSessionStore<TStore>() where TStore : class, Authentication.ISessionStore;
}
