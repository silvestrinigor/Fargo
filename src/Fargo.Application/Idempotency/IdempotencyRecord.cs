using Fargo.Core.Entities;

namespace Fargo.Application.Idempotency;

/// <summary>
/// The first result of an idempotent request, kept so a retry with the same key can be answered without
/// running the handler again.
/// </summary>
/// <remarks>
/// Stored in the same transaction as the change it protects, so a record exists only if the change
/// committed. A failed request leaves no record and can be retried. A record is looked up by actor and key,
/// so one actor can never read or reuse another's.
/// </remarks>
public sealed class IdempotencyRecord : IConcurrencyVersioned
{
    public const string PrimaryKeyName = "pk_idempotency_keys";

    public const int CommandNameMaxLength = 200;

    public Guid ActorGuid { get; private init; }

    public Guid Key { get; private init; }

    /// <summary>
    /// The command the key was first used for. A key reused for another command is rejected, because its
    /// stored response would not fit.
    /// </summary>
    public string CommandName { get; private set; } = null!;

    /// <summary>
    /// The serialized response of the first run, or <see langword="null"/> when the command has none.
    /// </summary>
    public string? Response { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    // Required by EF Core.
    private IdempotencyRecord() { }

    public static IdempotencyRecord Create(
        Guid actorGuid,
        Guid key,
        string commandName,
        string? response,
        DateTimeOffset now,
        TimeSpan retention)
        => new()
        {
            ActorGuid = actorGuid,
            Key = key,
            CommandName = commandName,
            Response = response,
            CreatedAt = now,
            ExpiresAt = now + retention,
        };

    public bool IsExpiredAt(DateTimeOffset now) => ExpiresAt <= now;

    public bool IsFor(string commandName) => string.Equals(CommandName, commandName, StringComparison.Ordinal);

    /// <summary>
    /// Reuses an expired record for a new request. Two requests racing to do so cannot both succeed: the
    /// row's concurrency token lets only one save win.
    /// </summary>
    public void Renew(string commandName, string? response, DateTimeOffset now, TimeSpan retention)
    {
        CommandName = commandName;
        Response = response;
        CreatedAt = now;
        ExpiresAt = now + retention;
    }
}
