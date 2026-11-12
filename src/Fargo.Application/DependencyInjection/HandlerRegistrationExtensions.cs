using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Decorators;
using Fargo.Application.Idempotency;
using Fargo.Application.Persistence;
using Fargo.Core.Audits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.DependencyInjection;

/// <remarks>
/// Commands resolve to <c>Logging(unit of work(Audit(handler)))</c>, with the idempotency decorator between
/// the unit of work and the audit for a command marked <see cref="IIdempotentCommand"/>; queries resolve to
/// the logging decorator only. A command that implements <see cref="ITransactionalCommand"/> gets the
/// transactional decorator (begin, handler, save, commit) instead of the commit-on-success one; the
/// choice follows the command type, so it cannot be lost by calling the wrong helper.
///
/// The current actor is null in actor-less hosts (the initializer worker), where <see cref="ICurrentActor"/>
/// is not registered.
/// </remarks>
public static class HandlerRegistrationExtensions
{
    public static IServiceCollection AddFargoCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(sp =>
            new LoggingCommandHandlerDecorator<TCommand>(
                BuildUnitOfWorkDecorator<TCommand>(
                    BuildIdempotencyDecorator<TCommand>(
                        new AuditCommandHandlerDecorator<TCommand>(
                            sp.GetRequiredService<THandler>(),
                            sp.GetService<ICurrentActor>(),
                            sp.GetRequiredService<IAuditLogRepository>()),
                        sp),
                    sp.GetRequiredService<IUnitOfWork>()),
                sp.GetService<ICurrentActor>(),
                sp.GetRequiredService<ILogger<TCommand>>()));

        return services;
    }

    public static IServiceCollection AddFargoCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(sp =>
            new LoggingCommandHandlerDecorator<TCommand, TResponse>(
                BuildUnitOfWorkDecorator<TCommand, TResponse>(
                    BuildIdempotencyDecorator<TCommand, TResponse>(
                        new AuditCommandHandlerDecorator<TCommand, TResponse>(
                            sp.GetRequiredService<THandler>(),
                            sp.GetService<ICurrentActor>(),
                            sp.GetRequiredService<IAuditLogRepository>()),
                        sp),
                    sp.GetRequiredService<IUnitOfWork>()),
                sp.GetService<ICurrentActor>(),
                sp.GetRequiredService<ILogger<TCommand>>()));

        return services;
    }

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

    // Adds the idempotency decorator for a command marked IIdempotentCommand, from the command type itself.
    private static ICommandHandler<TCommand> BuildIdempotencyDecorator<TCommand>(
        ICommandHandler<TCommand> handler, IServiceProvider services)
        where TCommand : ICommand
        => typeof(IIdempotentCommand).IsAssignableFrom(typeof(TCommand))
            ? new IdempotentCommandHandlerDecorator<TCommand>(handler, services.GetRequiredService<IdempotencyGate>())
            : handler;

    private static ICommandHandler<TCommand, TResponse> BuildIdempotencyDecorator<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> handler, IServiceProvider services)
        where TCommand : ICommand<TResponse>
        => typeof(IIdempotentCommand).IsAssignableFrom(typeof(TCommand))
            ? new IdempotentCommandHandlerDecorator<TCommand, TResponse>(handler, services.GetRequiredService<IdempotencyGate>())
            : handler;

    // Chooses the transactional decorator from the command type itself.
    private static ICommandHandler<TCommand> BuildUnitOfWorkDecorator<TCommand>(
        ICommandHandler<TCommand> handler, IUnitOfWork unitOfWork)
        where TCommand : ICommand
        => typeof(ITransactionalCommand).IsAssignableFrom(typeof(TCommand))
            ? new TransactionalCommandHandlerDecorator<TCommand>(handler, unitOfWork)
            : new UnitOfWorkCommandHandlerDecorator<TCommand>(handler, unitOfWork);

    private static ICommandHandler<TCommand, TResponse> BuildUnitOfWorkDecorator<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> handler, IUnitOfWork unitOfWork)
        where TCommand : ICommand<TResponse>
        => typeof(ITransactionalCommand).IsAssignableFrom(typeof(TCommand))
            ? new TransactionalCommandHandlerDecorator<TCommand, TResponse>(handler, unitOfWork)
            : new UnitOfWorkCommandHandlerDecorator<TCommand, TResponse>(handler, unitOfWork);
}
