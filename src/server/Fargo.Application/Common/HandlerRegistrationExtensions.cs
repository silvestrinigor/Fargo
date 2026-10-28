using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fargo.Application.Common;

/// <summary>
/// Registration helpers that wire a handler together with its logging decorator
/// so every command/query is observed uniformly (started / completed / failed
/// with duration) without per-handler logging code.
///
/// <para>
/// Each helper registers the concrete handler as itself and registers the
/// handler interface to resolve the <c>Logging*Decorator</c>, which in turn
/// depends on the concrete handler. All registrations are scoped, matching the
/// previous lifetimes.
/// </para>
/// </summary>
public static class HandlerRegistrationExtensions
{
    /// <summary>Registers a no-response command handler behind the logging decorator.</summary>
    public static IServiceCollection AddFargoCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(sp =>
            new LoggingCommandHandlerDecorator<TCommand>(
                sp.GetRequiredService<THandler>(),
                sp,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TCommand>>()));

        return services;
    }

    /// <summary>Registers a command handler with a response behind the logging decorator.</summary>
    public static IServiceCollection AddFargoCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(sp =>
            new LoggingCommandHandlerDecorator<TCommand, TResponse>(
                sp.GetRequiredService<THandler>(),
                sp,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TCommand>>()));

        return services;
    }

    /// <summary>Registers a query handler behind the logging decorator.</summary>
    public static IServiceCollection AddFargoQueryHandler<TQuery, TResponse, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResponse>>(sp =>
            new LoggingQueryHandlerDecorator<TQuery, TResponse>(
                sp.GetRequiredService<THandler>(),
                sp,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TQuery>>()));

        return services;
    }
}
