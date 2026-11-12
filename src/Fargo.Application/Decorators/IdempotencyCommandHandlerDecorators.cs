using Fargo.Application.Abstractions;
using Fargo.Application.Idempotency;
using System.Text.Json;

namespace Fargo.Application.Decorators;

/// <summary>
/// For a command marked <see cref="IIdempotentCommand"/>: a retry with the same key is answered from the
/// stored result, and the key of a first run is recorded in the same save as its change.
/// </summary>
/// <remarks>
/// Sits inside the unit of work and outside the audit decorator. The unit of work then commits the record with
/// the change, so a record exists only for work that committed, and a replay is never audited or published
/// again. Two requests racing on one key both reach the save, and the unique key lets only one commit.
/// </remarks>
public sealed class IdempotentCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> inner;
    private readonly IdempotencyGate gate;

    internal IdempotentCommandHandlerDecorator(ICommandHandler<TCommand> inner, IdempotencyGate gate)
    {
        this.inner = inner;
        this.gate = gate;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var scope = await gate.BeginAsync(AsIdempotent(command), typeof(TCommand).Name, cancellationToken);

        if (scope is { IsReplay: true })
        {
            return;
        }

        await inner.HandleAsync(command, cancellationToken);

        scope?.Complete(response: null);
    }

    private static IIdempotentCommand AsIdempotent(TCommand command)
        => command as IIdempotentCommand
            ?? throw new InvalidOperationException($"{typeof(TCommand).Name} is not an idempotent command.");
}

/// <summary>
/// The idempotency decorator for commands with a response, which it stores as JSON and returns on a replay.
/// </summary>
public sealed class IdempotentCommandHandlerDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> inner;
    private readonly IdempotencyGate gate;

    internal IdempotentCommandHandlerDecorator(ICommandHandler<TCommand, TResponse> inner, IdempotencyGate gate)
    {
        this.inner = inner;
        this.gate = gate;
    }

    public async Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var scope = await gate.BeginAsync(AsIdempotent(command), typeof(TCommand).Name, cancellationToken);

        if (scope is { IsReplay: true })
        {
            return JsonSerializer.Deserialize<TResponse>(
                scope.StoredResponse
                    ?? throw new InvalidOperationException(
                        $"The stored result of {typeof(TCommand).Name} has no response to replay."))!;
        }

        var response = await inner.HandleAsync(command, cancellationToken);

        scope?.Complete(JsonSerializer.Serialize(response));

        return response;
    }

    private static IIdempotentCommand AsIdempotent(TCommand command)
        => command as IIdempotentCommand
            ?? throw new InvalidOperationException($"{typeof(TCommand).Name} is not an idempotent command.");
}