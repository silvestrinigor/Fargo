using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Fargo.ClientHttp.Extensions;

/// <summary>
/// Service collection extensions for Kiota handlers.
/// </summary>
public static class KiotaServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Kiota handlers in DI so <see cref="AttachKiotaHandlers(IHttpClientBuilder)"/>
    /// can resolve them.
    /// </summary>
    public static IServiceCollection AddKiotaHandlers(this IServiceCollection services)
    {
        var kiotaHandlers = KiotaClientFactory.GetDefaultHandlerActivatableTypes();
        foreach (var handler in kiotaHandlers)
        {
            services.AddTransient(handler);
        }

        return services;
    }

    /// <summary>
    /// Adds the Kiota handlers to the client, in the order they run. Requires
    /// <see cref="AddKiotaHandlers(IServiceCollection)"/> to have registered them.
    /// </summary>
    public static IHttpClientBuilder AttachKiotaHandlers(this IHttpClientBuilder builder)
    {
        var kiotaHandlers = KiotaClientFactory.GetDefaultHandlerActivatableTypes();
        foreach (var handler in kiotaHandlers)
        {
            builder.AddHttpMessageHandler((sp) => (DelegatingHandler)sp.GetRequiredService(handler));
        }

        return builder;
    }
}
