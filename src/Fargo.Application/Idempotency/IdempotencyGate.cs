using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdempotentCommand = Fargo.Application.Abstractions.IIdempotentCommand;

namespace Fargo.Application.Idempotency;

/// <summary>
/// The decision the idempotency decorators share: run the command, or answer from the stored result.
/// </summary>
internal sealed class IdempotencyGate(
    IIdempotencyRecordRepository repository,
    IOptions<IdempotencyOptions> options,
    TimeProvider timeProvider,
    IdempotencyOutcome outcome,
    ILogger<IdempotencyGate> logger,
    ICurrentActor? currentActor = null)
{
    /// <returns>
    /// <see langword="null"/> when the request is not protected: it has no key, or there is no authenticated
    /// actor to scope the key to.
    /// </returns>
    /// <exception cref="FargoApplicationException">The key is the empty GUID.</exception>
    /// <exception cref="IdempotencyKeyReusedFargoApplicationException">
    /// The key was already used for a different command.
    /// </exception>
    public async Task<IdempotencyScope?> BeginAsync(
        IdempotentCommand command, string commandName, CancellationToken cancellationToken)
    {
        if (command.IdempotencyKey is not { } key || currentActor is not { IsAuthenticated: true })
        {
            return null;
        }

        if (key == Guid.Empty)
        {
            throw new FargoApplicationException(
                "The idempotency key cannot be the empty GUID.",
                FargoErrorType.InvalidOperation);
        }

        var now = timeProvider.GetUtcNow();

        var existing = await repository.FindAsync(currentActor.Guid, key, cancellationToken);

        if (existing is not null && !existing.IsExpiredAt(now))
        {
            if (!existing.IsFor(commandName))
            {
                throw new IdempotencyKeyReusedFargoApplicationException(key, existing.CommandName);
            }

            outcome.MarkReplayed();

            logger.IdempotentReplay(commandName, currentActor.Guid);

            return IdempotencyScope.Replay(existing.Response);
        }

        return IdempotencyScope.Run(
            repository, currentActor.Guid, key, commandName, now, options.Value.Retention, existing);
    }
}

internal sealed class IdempotencyScope
{
    private readonly IIdempotencyRecordRepository? repository;
    private readonly Guid actorGuid;
    private readonly Guid key;
    private readonly string commandName = null!;
    private readonly DateTimeOffset now;
    private readonly TimeSpan retention;
    private readonly IdempotencyRecord? expired;

    public bool IsReplay { get; }

    /// <summary>
    /// The stored response of the first run; <see langword="null"/> when the command has none.
    /// </summary>
    public string? StoredResponse { get; }

    private IdempotencyScope(string? storedResponse)
    {
        IsReplay = true;
        StoredResponse = storedResponse;
    }

    private IdempotencyScope(
        IIdempotencyRecordRepository repository,
        Guid actorGuid,
        Guid key,
        string commandName,
        DateTimeOffset now,
        TimeSpan retention,
        IdempotencyRecord? expired)
    {
        this.repository = repository;
        this.actorGuid = actorGuid;
        this.key = key;
        this.commandName = commandName;
        this.now = now;
        this.retention = retention;
        this.expired = expired;
    }

    public static IdempotencyScope Replay(string? storedResponse) => new(storedResponse);

    public static IdempotencyScope Run(
        IIdempotencyRecordRepository repository,
        Guid actorGuid,
        Guid key,
        string commandName,
        DateTimeOffset now,
        TimeSpan retention,
        IdempotencyRecord? expired)
        => new(repository, actorGuid, key, commandName, now, retention, expired);

    /// <summary>
    /// Records the result in the context, so it is saved with the change the command made.
    /// </summary>
    public void Complete(string? response)
    {
        if (expired is not null)
        {
            expired.Renew(commandName, response, now, retention);

            return;
        }

        repository!.Add(IdempotencyRecord.Create(actorGuid, key, commandName, response, now, retention));
    }
}
