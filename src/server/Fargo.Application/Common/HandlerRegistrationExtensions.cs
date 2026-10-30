using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fargo.Application.Common;

/// <summary>
/// Registration helpers that wire a handler together with its cross-cutting
/// decorators so every command/query is observed uniformly (started / completed
/// / failed with duration) and every command commits its unit of work on
/// success, without per-handler logging or save code.
///
/// <para>
/// Each helper registers the concrete handler as itself and registers the
/// handler interface to resolve the decorator chain. For commands the resolved
/// chain is
/// <c>LoggingCommandHandlerDecorator(UnitOfWorkCommandHandlerDecorator(concreteHandler))</c>
/// — logging outermost, the commit-on-success unit-of-work decorator in the
/// middle, and the concrete handler innermost. Queries resolve only the logging
/// decorator. All registrations are scoped, matching the previous lifetimes.
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
                new UnitOfWorkCommandHandlerDecorator<TCommand>(
                    sp.GetRequiredService<THandler>(),
                    sp.GetRequiredService<IUnitOfWork>()),
                sp,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TCommand>>()));

        return services;
    }

    /// <summary>
    /// Registers a no-response command handler behind the logging decorator with
    /// a transactional unit-of-work decorator instead of the default
    /// commit-on-success one. The resolved chain is
    /// <c>LoggingCommandHandlerDecorator(TransactionalCommandHandlerDecorator(concreteHandler))</c>,
    /// so the handler runs inside a single explicit transaction (begin → handler
    /// → save → commit). Use this for commands whose handler acquires a
    /// transaction-scoped database lock that must be held until the commit.
    /// </summary>
    public static IServiceCollection AddFargoTransactionalCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(sp =>
            new LoggingCommandHandlerDecorator<TCommand>(
                new TransactionalCommandHandlerDecorator<TCommand>(
                    sp.GetRequiredService<THandler>(),
                    sp.GetRequiredService<IUnitOfWork>()),
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
                new UnitOfWorkCommandHandlerDecorator<TCommand, TResponse>(
                    sp.GetRequiredService<THandler>(),
                    sp.GetRequiredService<IUnitOfWork>()),
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
