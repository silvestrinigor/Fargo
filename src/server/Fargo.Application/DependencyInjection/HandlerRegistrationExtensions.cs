using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Decorators;
using Fargo.Application.Persistence;
using Fargo.Core.Audits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;


namespace Fargo.Application.DependencyInjection;

/// <summary>
/// Registration helpers that wire a handler together with its cross-cutting
/// decorators so every command/query is observed uniformly (started / completed
/// / failed with duration) and every command commits its unit of work, without
/// per-handler logging or save code.
///
/// <para>
/// Each helper registers the concrete handler as itself and registers the
/// handler interface to resolve the decorator chain. For commands the resolved
/// chain is
/// <c>LoggingCommandHandlerDecorator(&lt;unit-of-work decorator&gt;(concreteHandler))</c>
/// — logging outermost, the unit-of-work decorator in the middle, and the
/// concrete handler innermost. The middle decorator is chosen automatically from
/// the command type: a command implementing <see cref="ITransactionalCommand"/>
/// gets the transactional (begin → handler → save → commit) decorator, every
/// other command gets the commit-on-success decorator. Because the choice is
/// driven by the command's own type it cannot be lost by registering with the
/// wrong helper. Queries resolve only the logging decorator. All registrations
/// are scoped.
/// </para>
///
/// <para>
/// The optional current actor used by the logging decorators is resolved from the
/// scope via <see cref="ServiceProviderServiceExtensions.GetService{T}"/> — it is
/// <see langword="null"/> in actor-less hosts (the seed worker) where
/// <see cref="ICurrentActor"/> is not registered — and passed to the decorator as
/// a typed dependency, so the decorator itself holds no service-provider reference.
/// </para>
/// </summary>
public static class HandlerRegistrationExtensions
{
    /// <summary>Registers a no-response command handler behind the logging + unit-of-work chain.</summary>
    public static IServiceCollection AddFargoCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(sp =>
            new LoggingCommandHandlerDecorator<TCommand>(
                BuildUnitOfWorkDecorator<TCommand>(
                    new AuditCommandHandlerDecorator<TCommand>(
                        sp.GetRequiredService<THandler>(),
                        sp.GetService<ICurrentActor>(),
                        sp.GetRequiredService<IAuditLogRepository>()),
                    sp.GetRequiredService<IUnitOfWork>()),
                sp.GetService<ICurrentActor>(),
                sp.GetRequiredService<ILogger<TCommand>>()));

        return services;
    }

    /// <summary>Registers a command handler with a response behind the logging + unit-of-work chain.</summary>
    public static IServiceCollection AddFargoCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(sp =>
            new LoggingCommandHandlerDecorator<TCommand, TResponse>(
                new UnitOfWorkCommandHandlerDecorator<TCommand, TResponse>(
                    new AuditCommandHandlerDecorator<TCommand, TResponse>(
                        sp.GetRequiredService<THandler>(),
                        sp.GetService<ICurrentActor>(),
                        sp.GetRequiredService<IAuditLogRepository>()),
                    sp.GetRequiredService<IUnitOfWork>()),
                sp.GetService<ICurrentActor>(),
                sp.GetRequiredService<ILogger<TCommand>>()));

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
                sp.GetService<ICurrentActor>(),
                sp.GetRequiredService<ILogger<TQuery>>()));

        return services;
    }

    // Selects the unit-of-work decorator for a no-response command from the command
    // type itself: a transactional command (begin → handler → save → commit) when it
    // implements ITransactionalCommand, otherwise the commit-on-success decorator.
    private static ICommandHandler<TCommand> BuildUnitOfWorkDecorator<TCommand>(
        ICommandHandler<TCommand> handler, IUnitOfWork unitOfWork)
        where TCommand : ICommand
        => typeof(ITransactionalCommand).IsAssignableFrom(typeof(TCommand))
            ? new TransactionalCommandHandlerDecorator<TCommand>(handler, unitOfWork)
            : new UnitOfWorkCommandHandlerDecorator<TCommand>(handler, unitOfWork);
}
