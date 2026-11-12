namespace Fargo.Infrastructure.Persistence;

/// <summary>
/// The single row whose <see cref="Value"/> changes, in the same transaction, whenever data that
/// affects any actor's permissions or partition access changes (see
/// <see cref="AuthorizationStampSaveChangesInterceptor"/>). Cached authorization snapshots are keyed
/// by it, so a change invalidates them on every instance without contacting the cache.
/// </summary>
/// <remarks>
/// A new random value rather than a counter: two concurrent changes could both raise a counter to the
/// same number, leaving an entry computed between them looking current. A new value never repeats.
/// </remarks>
public sealed class AuthorizationStamp
{
    public const int SingletonId = 1;

    /// <summary>
    /// The value seeded by the migration.
    /// </summary>
    public static readonly Guid InitialValue = Guid.Parse("6b2f3c1e-9a4d-4e7b-8c5f-1d2e3f4a5b6c");

    public int Id { get; private init; } = SingletonId;

    public Guid Value { get; private set; }

    // Required by EF Core.
    private AuthorizationStamp() { }

    internal AuthorizationStamp(Guid value)
    {
        Value = value;
    }

    internal void Renew() => Value = Guid.NewGuid();
}
