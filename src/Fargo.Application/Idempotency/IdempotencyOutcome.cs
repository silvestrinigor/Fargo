namespace Fargo.Application.Idempotency;

/// <summary>
/// Tells the HTTP layer how the idempotency decorator treated the current request, so it can mark a replay.
/// </summary>
public sealed class IdempotencyOutcome
{
    public bool Replayed { get; private set; }

    public void MarkReplayed() => Replayed = true;
}