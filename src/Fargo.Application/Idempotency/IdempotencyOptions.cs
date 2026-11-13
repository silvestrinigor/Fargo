namespace Fargo.Application.Idempotency;

public sealed class IdempotencyOptions
{
    public const string SectionName = "Idempotency";

    /// <summary>
    /// How long a key protects a request. After that the key may be used again.
    /// </summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromHours(24);
}
