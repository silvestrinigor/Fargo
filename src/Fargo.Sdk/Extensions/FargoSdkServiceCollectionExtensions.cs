using Fargo.GrpcClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fargo.Sdk.Extensions;

/// <summary>
/// Extension methods to register the Fargo command SDK.
/// </summary>
public static class FargoSdkServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Fargo gRPC workspace transport and command client.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Delegate to configure <see cref="FargoSdkOptions"/>.</param>
    /// <param name="lifetime">
    /// Lifetime to use for the typed clients. Use <see cref="ServiceLifetime.Scoped"/> for
    /// Blazor Server (one set per circuit), <see cref="ServiceLifetime.Singleton"/> for
    /// background services and MCP tools.
    /// </param>
    public static IServiceCollection AddFargoSdk(
        this IServiceCollection services,
        Action<FargoSdkOptions> configure,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var options = new FargoSdkOptions();
        configure(options);
        services.TryAddSingleton(options);

        services.AddFargoGrpcClient(grpc =>
        {
            grpc.Address = options.Address;
            grpc.DefaultTimeout = options.DefaultTimeout;
            grpc.BearerTokenProvider = options.BearerTokenProvider;
        }, lifetime);

        services.Add(ServiceDescriptor.Describe(
            typeof(IFargoCommandClient),
            typeof(FargoCommandClient),
            lifetime));

        return services;
    }
}
