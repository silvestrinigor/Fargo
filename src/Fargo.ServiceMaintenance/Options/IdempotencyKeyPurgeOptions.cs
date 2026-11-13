using System.ComponentModel.DataAnnotations;

namespace Fargo.ServiceMaintenance.Options;

/// <remarks>
/// There is no retention setting here: every record stores its own expiry, written by the API from its
/// <c>Idempotency:Retention</c> setting, so the two services can never disagree about when a key ends.
/// </remarks>
public sealed class IdempotencyKeyPurgeOptions
{
    public const string SectionName = "Maintenance:IdempotencyKeys";

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan PurgeInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Records deleted per statement, so one purge never holds locks for long.
    /// </summary>
    [Range(1, 100_000)]
    public int BatchSize { get; set; } = 1000;
}
