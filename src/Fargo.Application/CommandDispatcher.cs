using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application;

public sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task Dispatch<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        var behaviors = serviceProvider.GetServices<ICommandBehavior<TCommand>>().Reverse().ToArray();

        Task InvokeHandler(CancellationToken ct)
            => handler.Handle(command, ct);

        var pipeline = behaviors.Aggregate(
            (Func<CancellationToken, Task>)InvokeHandler,
            (next, behavior) => ct => behavior.Handle(command, next, ct));

        await pipeline(cancellationToken);
    }

    public async Task<TResponse> Dispatch<TCommand, TResponse>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        var behaviors = serviceProvider.GetServices<ICommandBehavior<TCommand, TResponse>>().Reverse().ToArray();

        Task<TResponse> InvokeHandler(CancellationToken ct)
            => handler.Handle(command, ct);

        var pipeline = behaviors.Aggregate(
            (Func<CancellationToken, Task<TResponse>>)InvokeHandler,
            (next, behavior) => ct => behavior.Handle(command, next, ct));

        return await pipeline(cancellationToken);
    }

    public async Task<object?> Dispatch(
        object command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();
        var responseInterface = commandType
            .GetInterfaces()
            .SingleOrDefault(static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        if (responseInterface is not null)
        {
            var responseType = responseInterface.GetGenericArguments()[0];
            var method = typeof(CommandDispatcher)
                .GetMethods()
                .Single(m => m.Name == nameof(Dispatch) &&
                    m.IsGenericMethodDefinition &&
                    m.GetGenericArguments().Length == 2);

            var task = (Task)method
                .MakeGenericMethod(commandType, responseType)
                .Invoke(this, [command, cancellationToken])!;

            await task.ConfigureAwait(false);

            return task
                .GetType()
                .GetProperty(nameof(Task<object>.Result))!
                .GetValue(task);
        }

        if (command is not ICommand)
        {
            throw new ArgumentException(
                $"Command type '{commandType.FullName}' does not implement ICommand.",
                nameof(command));
        }

        var noResultMethod = typeof(CommandDispatcher)
            .GetMethods()
            .Single(m => m.Name == nameof(Dispatch) &&
                m.IsGenericMethodDefinition &&
                m.GetGenericArguments().Length == 1);

        var noResultTask = (Task)noResultMethod
            .MakeGenericMethod(commandType)
            .Invoke(this, [command, cancellationToken])!;

        await noResultTask.ConfigureAwait(false);

        return null;
    }
}
